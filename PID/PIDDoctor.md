# PID Doctor:
- SP + PV (+ opsiyonel valf) üzerinden loop davranışını analiz edecek,
- KPI’ları (hata, osilasyon, noise, saturasyon vb.) çıkaracak,
- Sonunda da “daha çok Kp, daha az Ki, biraz Kd” gibi yönlü öneriler yapacak.
- SP sabit veya yavaş değişen loop’lar için; valf tag’i varsa kullanıyor, yoksa sadece SP–PV’den teşhis yapıyor.
```csharp
// ==============================
// PID DOCTOR – Loop Behaviour & Tuning Hints
// ==============================
using System;
using System.Linq;
// ---- 0) KONFİG ----
// Zorunlu tag'ler
string spTag = "GVL_Var.fTempSet";
string pvTag = "GVL_Var.fTempActual";
// Opsiyonel: analog valf çıkışı (0..100). Yoksa "" bırak.
string valveTag = "GVL_Var.fValveActual"; // veya "" (kullanmayacaksan)
// Opsiyonel: PID parametre tag'leri (sadece varsa okunur, yoksa davranış üzerinden öneri yapılır)
string kpTag = "GVL_Var.fPID_Kp";
string kiTag = "GVL_Var.fPID_Ki";
string kdTag = "GVL_Var.fPID_Kd";
// Analiz penceresi (örnek sayısı)
int n = 300;
// Örnekleme süresi tahmini (saniye) – sadece bilgi amaçlı
double sampleTimeSec = 1.0;
// ---- 1) PID parametrelerini (varsa) oku ----
double? kpVal = !string.IsNullOrWhiteSpace(kpTag) ? H.LastDouble(kpTag) : null;
double? kiVal = !string.IsNullOrWhiteSpace(kiTag) ? H.LastDouble(kiTag) : null;
double? kdVal = !string.IsNullOrWhiteSpace(kdTag) ? H.LastDouble(kdTag) : null;
double kp = kpVal ?? 0.0;
double ki = kiVal ?? 0.0;
double kd = kdVal ?? 0.0;
bool integralUsed   = ki > 0.0;
bool derivativeUsed = kd > 0.0;
// ---- 2) SP / PV / Valve history ----
var spHistRaw = H.LastN(spTag, n);
var pvHistRaw = H.LastN(pvTag, n);
if (spHistRaw == null || pvHistRaw == null || spHistRaw.Length == 0 || pvHistRaw.Length == 0)
{
    H.Console($"PID-DOCTOR: SP/PV history yok veya boş. SP={spHistRaw?.Length ?? 0}, PV={pvHistRaw?.Length ?? 0}");
    return;
}
// SP & PV'yi hizala ve null/parse edilemeyenleri temizle
int rawMin = Math.Min(spHistRaw.Length, pvHistRaw.Length);
var spList = new System.Collections.Generic.List<double>();
var pvList = new System.Collections.Generic.List<double>();
for (int i = 0; i < rawMin; i++)
{
    var sv = H.Double(spHistRaw[i]);
    var pv = H.Double(pvHistRaw[i]);
    if (sv.HasValue && pv.HasValue)
    {
        spList.Add(sv.Value);
        pvList.Add(pv.Value);
    }
}
int len = Math.Min(spList.Count, pvList.Count);
if (len < 60)
{
    H.Console($"PID-DOCTOR: Yetersiz ortak örnek (len={len} < 60). SPraw={spHistRaw.Length}, PVraw={pvHistRaw.Length}");
    return;
}
// Gerekirse son n'i kullan
if (len > n)
{
    spList = spList.Skip(len - n).ToList();
    pvList = pvList.Skip(len - n).ToList();
    len = n;
}
double[] spVals = spList.ToArray();
double[] pvVals = pvList.ToArray();
// Valve (opsiyonel)
double[]? valveVals = null;
bool valveAvailable = !string.IsNullOrWhiteSpace(valveTag);
int rawValveCount = 0;
if (valveAvailable)
{
    var valveHistRaw = H.LastN(valveTag, n);
    if (valveHistRaw != null && valveHistRaw.Length > 0)
    {
        rawValveCount = valveHistRaw.Length;
        int rawMinValve = Math.Min(valveHistRaw.Length, len);
        var vList = new System.Collections.Generic.List<double>();
        for (int i = 0; i < rawMinValve; i++)
        {
            var vv = H.Double(valveHistRaw[i]);
            if (vv.HasValue)
                vList.Add(vv.Value);
        }
        if (vList.Count > 0)
        {
            // Uzunlukları eşitle
            int eff = Math.Min(len, vList.Count);
            valveVals = vList.Skip(vList.Count - eff).Take(eff).ToArray();
            // SP/PV de aynı efektif uzunluğa çekilsin
            if (eff < len)
            {
                spVals = spVals.Skip(len - eff).Take(eff).ToArray();
                pvVals = pvVals.Skip(len - eff).Take(eff).ToArray();
                len = eff;
            }
        }
    }
}
// ---- 3) Temel istatistikler ----
double spAvg = spVals.Average();
double pvAvg = pvVals.Average();
double spStd;
{
    double m = spAvg;
    double sumSq = spVals.Sum(v => (v - m) * (v - m));
    spStd = Math.Sqrt(sumSq / spVals.Length);
}
double pvStd;
{
    double m = pvAvg;
    double sumSq = pvVals.Sum(v => (v - m) * (v - m));
    pvStd = Math.Sqrt(sumSq / pvVals.Length);
}
double spAbs = Math.Max(Math.Abs(spAvg), 1e-6);
// SP stabil mi?
double spStdThresh = Math.Max(spAbs * 0.01, 0.01); // SP'nin %1'i veya min 0.01
bool spStable = spStd <= spStdThresh;
// Hata sinyali: e = SP - PV
double[] err = new double[len];
for (int i = 0; i < len; i++)
    err[i] = spVals[i] - pvVals[i];
double errAvg     = err.Average();
double errAbsMean = err.Select(Math.Abs).Average();
double errRms     = Math.Sqrt(err.Select(e => e * e).Average());
double errVar = err.Sum(e => (e - errAvg) * (e - errAvg)) / err.Length;
double errStd = Math.Sqrt(errVar);
// Hata işaret değişim sayısı → osilasyon göstergesi
int errZeroCross = 0;
for (int i = 1; i < len; i++)
{
    if (Math.Sign(err[i]) != 0 && Math.Sign(err[i - 1]) != 0 &&
        Math.Sign(err[i]) != Math.Sign(err[i - 1]))
        errZeroCross++;
}
// PV max/min → overshoot / undershoot
double pvMax = pvVals.Max();
double pvMin = pvVals.Min();
// Valve istatistikleri (varsa)
double valveAvg = 0.0, valveStd = 0.0, valveHiRatio = 0.0, valveLoRatio = 0.0;
int valveToggles = 0;
if (valveVals != null && valveVals.Length > 1)
{
    valveAvg = valveVals.Average();
    {
        double m = valveAvg;
        double sumSq = valveVals.Sum(v => (v - m) * (v - m));
        valveStd = Math.Sqrt(sumSq / valveVals.Length);
    }
    valveHiRatio = valveVals.Count(v => v >= 90.0) / (double)valveVals.Length;
    valveLoRatio = valveVals.Count(v => v <= 10.0) / (double)valveVals.Length;
    for (int i = 1; i < valveVals.Length; i++)
    {
        if (Math.Abs(valveVals[i] - valveVals[i - 1]) > 0.5)
            valveToggles++;
    }
}
// ---- 4) Performans skorları ----
// 4.1 Steady-State Error Score (0–100)
double sseScore;
{
    double sseRefSmall = Math.Max(spAbs * 0.02, 0.1); // “iyi” hata seviyesi
    double sseRefBad   = Math.Max(spAbs * 0.10, 0.5); // “kötü” hata seviyesi
    double sseNorm     = Math.Min(errAbsMean / sseRefBad, 1.0);
    sseScore           = (1.0 - sseNorm) * 100.0;
}
// 4.2 Stability/Oscillation Score (0–100)
double oscIndex = errStd / (Math.Abs(errAvg) + 1e-3); // ~1 üzeri: dalgalı
double oscNorm  = Math.Clamp((oscIndex - 0.5) / (2.0 - 0.5), 0.0, 1.0);
double zeroCrossNorm = Math.Clamp((double)errZeroCross / (len / 5.0), 0.0, 2.0);
zeroCrossNorm = Math.Min(zeroCrossNorm, 1.0);
double stabilityScore = (1.0 - 0.5 * (oscNorm + zeroCrossNorm)) * 100.0;
// 4.3 Noise Score (PV jitter)
double noiseScore;
{
    double jitterRatio = pvStd / spAbs;
    double jitterGood  = 0.01;  // %1
    double jitterBad   = 0.10;  // %10
    double jitterNorm  = (jitterRatio - jitterGood) / (jitterBad - jitterGood);
    jitterNorm         = Math.Clamp(jitterNorm, 0.0, 1.0);
    noiseScore         = (1.0 - jitterNorm) * 100.0;
}
// 4.4 Valve Effort Score (gereksiz manipülasyon?)
double valveEffortScore = 100.0;
if (valveVals != null && valveVals.Length > 1)
{
    double effNorm = Math.Clamp(valveStd / 30.0, 0.0, 1.0); // 30% std üzeri agresif say
    valveEffortScore = (1.0 - effNorm) * 100.0;
}
double pidPerformance =
    0.4 * sseScore +
    0.3 * stabilityScore +
    0.2 * noiseScore +
    0.1 * valveEffortScore;
// ---- 5) PID Doctor – yorumlar & öneriler ----
var findings    = new System.Collections.Generic.List<string>();
var suggestions = new System.Collections.Generic.List<string>();
// Genel bulgular
if (!spStable)
    findings.Add("SP belirgin şekilde değişken; tuning yorumu adım/deney ortamında daha anlamlı olur.");
if (sseScore < 60.0)
    findings.Add("Kalıcı hata (offset) görece yüksek.");
if (stabilityScore < 60.0)
    findings.Add("Loop kararsız veya belirgin osilasyonlar var.");
if (noiseScore < 60.0)
    findings.Add("PV üzerinde ciddi gürültü / jitter var.");
if (valveVals != null && (valveHiRatio > 0.6 || valveLoRatio > 0.6))
    findings.Add("Valf uzun süre saturasyonda kalıyor (0% veya 100% yakınında).");
// Kp değerlendirme
if (sseScore < 60.0 && stabilityScore > 75.0 && valveEffortScore > 70.0)
{
    suggestions.Add("Kp muhtemelen düşük: adım adım (örneğin %10–20) artırmayı deneyebilirsin; overshoot ve osilasyonu izleyerek.");
}
else if (stabilityScore < 60.0 || oscIndex > 1.5)
{
    suggestions.Add("Loop osilasyonlu/kararsız: Kp'yi kademeli olarak (%10–20) azalt, gerekirse Ki'yi de düşür.");
}
// Ki değerlendirme
bool longHighSat = valveHiRatio > 0.6 || valveLoRatio > 0.6;
if (integralUsed)
{
    if (sseScore < 60.0 && !longHighSat)
        suggestions.Add("Kalıcı offset görülüyor, valf saturasyonda değil: Ki'yi biraz artırmak (veya integral zamanını kısaltmak) yardımcı olabilir.");
    if (longHighSat && sseScore < 80.0)
        suggestions.Add("Valf uzun süre saturasyonda ve hata büyük: Ki fazla agresif olabilir; Ki'yi azalt veya anti-windup uygulamasını gözden geçir.");
}
else
{
    if (sseScore < 70.0)
        suggestions.Add("Integral (Ki) devre dışı; kalıcı offseti azaltmak için küçük bir Ki eklemeyi düşünebilirsin.");
}
// Kd değerlendirme
if (derivativeUsed)
{
    if (noiseScore < 50.0 && pvStd > spAbs * 0.05)
        suggestions.Add("PV gürültülü ve Kd aktif: Kd'yi azalt veya D bloğunu daha fazla filtrele (örneğin daha uzun D filtresi).");
    else if (stabilityScore < 70.0 && oscIndex > 1.2 && noiseScore > 70.0)
        suggestions.Add("Osilasyon var ancak PV gürültüsü makul: Kd'yi hafif artırmak overshoot'u azaltabilir.");
}
else
{
    if (stabilityScore < 70.0 && noiseScore > 60.0)
        suggestions.Add("Overshoot/oscillation yüksek, gürültü makul: küçük bir Kd eklemek (ve uygun filtreyle) faydalı olabilir.");
}
// Valve bulunuyorsa mekanik/aktüatör bulguları
if (valveVals != null && valveVals.Length > 1)
{
    if (valveStd < 1.0 && sseScore < 70.0)
        findings.Add("Valf neredeyse hiç hareket etmiyor, fakat kalıcı hata var → valf stroke/ölçek veya kontrol sinyali tarafında problem olabilir.");
    if (valveToggles > valveVals.Length / 5)
        findings.Add("Valf çıkışı çok sık değişiyor (chatter eğilimi) → Kp/ Ki fazla agresif, ölü bant/filtre gerekebilir.");
}
// Genel düzeyde yorum
if (pidPerformance >= 80.0 && stabilityScore >= 80.0 && sseScore >= 80.0)
    findings.Add("Genel PID performansı iyi; sadece ince ayar seviyesinde iyileştirmeler gerekebilir.");
else if (pidPerformance < 50.0)
    findings.Add("PID performansı zayıf; temel tuning adımlarına (Kp/Ki) odaklanmak faydalı olacaktır.");
// ---- 6) Konsol raporu ----
H.Console("============================================");
H.Console("PID DOCTOR – Loop Health & Tuning Hints");
H.Console($"SP = {spTag}, PV = {pvTag}" + (valveVals != null ? $", Valve = {valveTag}" : ""));
if (kpVal.HasValue || kiVal.HasValue || kdVal.HasValue)
    H.Console($"Kp={kp:F4}, Ki={ki:F4}, Kd={kd:F4}");
else
    H.Console("Kp/Ki/Kd tag'leri okunamadı veya tanımlı değil; davranış üzerinden yorumlanıyor.");
H.Console($"Window: len={len} samples, dt≈{sampleTimeSec:F2}s");
H.Console("--------------------------------------------");
H.Console($"SPavg={spAvg:F3}, SPstd={spStd:F3}  → SP {(spStable ? "stabil" : "oynak")}");
H.Console($"PVavg={pvAvg:F3}, PVstd={pvStd:F3}, PVmax={pvMax:F3}, PVmin={pvMin:F3}");
H.Console($"ErrAvg={errAvg:F3}, |Err|mean={errAbsMean:F3}, ErrRMS={errRms:F3}, ErrStd={errStd:F3}, ZeroCross={errZeroCross}");
if (valveVals != null && valveVals.Length > 0)
{
    H.Console($"ValveAvg={valveAvg:F2}, ValveStd={valveStd:F2}, HiSat%={(valveHiRatio * 100.0):F1}, LoSat%={(valveLoRatio * 100.0):F1}, Toggles={valveToggles}");
}
else if (valveAvailable)
{
    H.Console($"Valve history yok veya çok yetersiz. rawValveCount={rawValveCount}");
}
H.Console("--------------------------------------------");
H.Console("Scores (0-100):");
H.Console($"  Steady-State Error      = {sseScore:F1}");
H.Console($"  Stability/Oscillation   = {stabilityScore:F1}");
H.Console($"  Noise (PV jitter)       = {noiseScore:F1}");
H.Console($"  Valve Effort            = {valveEffortScore:F1}");
H.Console($"  PID Performance Overall = {pidPerformance:F1}");
H.Console("--------------------------------------------");
H.Console("Bulgular:");
if (findings.Count == 0)
    H.Console("  Belirgin bir problem sinyali tespit edilmedi.");
else
{
    int i = 1;
    foreach (var f in findings)
        H.Console($"  {i++}. {f}");
}
H.Console("Öneriler (tuning yönü):");
if (suggestions.Count == 0)
{
    H.Console("  Net bir tuning önerisi çıkmadı; mevcut ayarlar davranışa göre nötr görünüyor.");
}
else
{
    int i = 1;
    foreach (var s in suggestions)
        H.Console($"  {i++}. {s}");
}
H.Console("============================================");
// CSV log:
// H.LogSample(
//     "LoopHealth",
//     "pid_doctor",
//     ("SPtag", spTag),
//     ("PVtag", pvTag),
//     ("ValveTag", valveTag),
//     ("Kp", kp),
//     ("Ki", ki),
//     ("Kd", kd),
//     ("SPavg", spAvg),
//     ("SPstd", spStd),
//     ("PVavg", pvAvg),
//     ("PVstd", pvStd),
//     ("ErrAvg", errAvg),
//     ("ErrAbsMean", errAbsMean),
//     ("ErrRMS", errRms),
//     ("ErrStd", errStd),
//     ("ErrZeroCross", errZeroCross),
//     ("ValveAvg", valveAvg),
//     ("ValveStd", valveStd),
//     ("ValveHiSatRatio", valveHiRatio),
//     ("ValveLoSatRatio", valveLoRatio),
//     ("ValveToggles", valveToggles),
//     ("SSEScore", sseScore),
//     ("StabilityScore", stabilityScore),
//     ("NoiseScore", noiseScore),
//     ("ValveEffortScore", valveEffortScore),
//     ("PIDPerformance", pidPerformance)
// );
// Script bir alarm üretmek istemiyorsa:
return;
```
