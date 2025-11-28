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
### PID Doctor
**SP sabit + analog valf için ekstra teşhisler:**
- SP, PV ve isteğe bağlı analog valf çıkışını (0–100) analiz eder,
- Kp/Ki/Kd varsa okur, yoksa sadece davranıştan yorum yapar,
- Ayrıntılı “bulgular” ve “öneriler”i H.Console ile yazar,
- Eğer loop ciddi kötü durumdaysa H.Hit("...") ile alarm üretir,
- Veri eksikse, SP çok oynaksa vs. sadece bilgi yazar ve false döner (programı asla düşürmez).

**PID DOCTOR – SP sabit + analog valf odaklı, alarm üreten sürüm**
-----------------
```csharp
// ==============================
// PID DOCTOR – Loop Behaviour & Tuning Hints (with Hit)
// ==============================
using System;
using System.Linq;
// ---- 0) KONFİG ----
// Zorunlu tag'ler
string spTag = "GVL_Var.fTempSet";
string pvTag = "GVL_Var.fTempActual";
// Opsiyonel: analog valf çıkışı (0..100). Yoksa "" bırak.
string valveTag = "GVL_Var.fValveActual"; // analog valve; boş bırakırsan sadece SP+PV analiz edilir
// Opsiyonel: PID parametre tag'leri (varsa okunur)
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
// ---- 2) SP / PV history ----
var spHistRaw = H.LastN(spTag, n);
var pvHistRaw = H.LastN(pvTag, n);
if (spHistRaw == null || pvHistRaw == null || spHistRaw.Length == 0 || pvHistRaw.Length == 0)
{
    H.Console($"PID-DOCTOR: SP/PV history yok veya boş. SP={spHistRaw?.Length ?? 0}, PV={pvHistRaw?.Length ?? 0}");
    return false;
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
    return false;
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
// ---- 3) Valve (opsiyonel, analog bekleniyor 0..100) ----
double[]? valveVals = null;
bool valveRequested = !string.IsNullOrWhiteSpace(valveTag);
int rawValveCount = 0;
if (valveRequested)
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
            int eff = Math.Min(len, vList.Count);
            valveVals = vList.Skip(vList.Count - eff).Take(eff).ToArray();
            if (eff < len)
            {
                spVals = spVals.Skip(len - eff).Take(eff).ToArray();
                pvVals = pvVals.Skip(len - eff).Take(eff).ToArray();
                len = eff;
            }
        }
    }
}
// ---- 4) Temel istatistikler ----
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
double spStdThresh = Math.Max(spAbs * 0.01, 0.01);  // SP'nin ~%1'i
bool spStable = spStd <= spStdThresh;
// "çok stabil" dediğimiz, step sonrası sabit değer: daha sıkı eşik
double spStrongStable = spStd <= Math.Max(spAbs * 0.005, 0.005);
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
// Approx overshoot (SP gerçekten sabitse daha anlamlı)
double overshootRatio = 0.0;
if (spStrongStable && spAbs > 1e-6)
{
    overshootRatio = (pvMax - spAvg) / spAbs;
}
// Valve istatistikleri (analog, 0..100 varsayımı)
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
// ---- 5) Performans skorları ----
// 5.1 Steady-State Error Score (0–100)
double sseScore;
{
    double sseRefSmall = Math.Max(spAbs * 0.02, 0.1); // “iyi” hata seviyesi
    double sseRefBad   = Math.Max(spAbs * 0.10, 0.5); // “kötü” hata seviyesi
    double sseNorm     = Math.Min(errAbsMean / sseRefBad, 1.0);
    sseScore           = (1.0 - sseNorm) * 100.0;
}
// 5.2 Stability/Oscillation Score (0–100)
double oscIndex = errStd / (Math.Abs(errAvg) + 1e-3); // ~1 üzeri: dalgalı
double oscNorm  = Math.Clamp((oscIndex - 0.5) / (2.0 - 0.5), 0.0, 1.0);
double zeroCrossNorm = Math.Clamp((double)errZeroCross / (len / 5.0), 0.0, 2.0);
zeroCrossNorm = Math.Min(zeroCrossNorm, 1.0);
double stabilityScore = (1.0 - 0.5 * (oscNorm + zeroCrossNorm)) * 100.0;
// 5.3 Noise Score (PV jitter)
double noiseScore;
{
    double jitterRatio = pvStd / spAbs;
    double jitterGood  = 0.01;  // %1
    double jitterBad   = 0.10;  // %10
    double jitterNorm  = (jitterRatio - jitterGood) / (jitterBad - jitterGood);
    jitterNorm         = Math.Clamp(jitterNorm, 0.0, 1.0);
    noiseScore         = (1.0 - jitterNorm) * 100.0;
}
// 5.4 Valve Effort Score (gereksiz manipülasyon?)
double valveEffortScore = 100.0;
if (valveVals != null && valveVals.Length > 1)
{
    double effNorm = Math.Clamp(valveStd / 30.0, 0.0, 1.0); // 30% std üstü agresif say
    valveEffortScore = (1.0 - effNorm) * 100.0;
}
// Toplam PID performans skoru
double pidPerformance =
    0.4 * sseScore +
    0.3 * stabilityScore +
    0.2 * noiseScore +
    0.1 * valveEffortScore;
// ---- 6) PID Doctor – bulgular & öneriler (SP sabit + analog valf için ekstra teşhisler dahil) ----
var findings    = new System.Collections.Generic.List<string>();
var suggestions = new System.Collections.Generic.List<string>();
// Genel bulgular
if (!spStable)
    findings.Add("SP belirgin şekilde değişken; tuning yorumu step test veya sabit setpoint koşulunda daha anlamlı olur.");
if (sseScore < 60.0)
    findings.Add("Kalıcı hata (offset) görece yüksek.");
if (stabilityScore < 60.0)
    findings.Add("Loop kararsız veya belirgin osilasyonlar var.");
if (noiseScore < 60.0)
    findings.Add("PV üzerinde belirgin gürültü / jitter var.");
if (valveVals != null && (valveHiRatio > 0.6 || valveLoRatio > 0.6))
    findings.Add("Valf uzun süre saturasyonda (0% veya 100% civarı) kalıyor.");
// SP sabitse ve analog valf varsa ekstra teşhisler:
if (spStrongStable && valveVals != null && valveVals.Length > 10)
{
    // 1) Overshoot analizi
    if (overshootRatio > 0.2)
        findings.Add($"Step sonrası overshoot yüksek görünüyor (yaklaşık %{overshootRatio * 100.0:F1}).");
    else if (overshootRatio > 0.05)
        findings.Add($"Step sonrası overshoot orta seviyede (yaklaşık %{overshootRatio * 100.0:F1}).");
    // 2) “Sluggish” (yavaş cevap) analizi: yüksek hata + düşük valf hareketi
    if (sseScore < 60.0 && valveEffortScore > 70.0)
        findings.Add("Hata belirgin, valf nispeten sakin → loop muhtemelen yavaş/tembel (Kp düşük veya integral zayıf).");
    // 3) Fazla agresif davranış: yüksek valfStd + osilasyon
    if (valveEffortScore < 50.0 && stabilityScore < 70.0)
        findings.Add("Valf çıkışı çok hareketli ve loop dalgalı → Kp / Ki / Kd fazla agresif olabilir.");
    // 4) Olası stiction / ölü bant
    if (valveStd > 5.0 && pvStd < spAbs * 0.01 && sseScore < 70.0)
        findings.Add("Valf hareket ediyor ama PV çok az değişiyor → olası stiction / mekanik problem / yanlış yön.");
}
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
        findings.Add("Valf çıkışı çok sık değişiyor (chatter eğilimi) → Kp/Ki fazla agresif, ölü bant/filtre gerekebilir.");
}
// Genel düzeyde yorum
if (pidPerformance >= 80.0 && stabilityScore >= 80.0 && sseScore >= 80.0)
    findings.Add("Genel PID performansı iyi; sadece ince ayar seviyesinde iyileştirmeler gerekebilir.");
else if (pidPerformance < 50.0)
    findings.Add("PID performansı zayıf; temel tuning adımlarına (Kp/Ki) odaklanmak faydalı olacaktır.");
// ---- 7) Konsol raporu ----
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
if (spStrongStable)
    H.Console($"Approx overshoot ratio ≈ {overshootRatio * 100.0:F1}% (SP sabit varsayımıyla).");
if (valveVals != null && valveVals.Length > 0)
{
    H.Console($"ValveAvg={valveAvg:F2}, ValveStd={valveStd:F2}, HiSat%={(valveHiRatio * 100.0):F1}, LoSat%={(valveLoRatio * 100.0):F1}, Toggles={valveToggles}");
}
else if (valveRequested)
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
// ---- 8) İsteğe bağlı CSV log (yorumdan çıkarabilirsin) ----
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
// ---- 9) Alarm (Hit) kararı ----
// Ciddi bir tuning/loop problemi varsa H.Hit(...) ile alarm üretelim.
bool severe =
    pidPerformance < 50.0 ||
    stabilityScore < 50.0 ||
    sseScore < 50.0 ||
    (spStrongStable && overshootRatio > 0.3) ||
    (valveVals != null && (valveHiRatio > 0.8 || valveLoRatio > 0.8) && sseScore < 70.0);
if (severe)
{
    return H.Hit("PID Doctor: loop ciddi tuning/performans problemi gösteriyor (detay için console/CSV'ye bak).");
}
// Alarm yok, sadece teşhis raporu
return false;
```
Bu haliyle:
- Script hiçbir null erişim hatasıyla programı düşürmez.
- SP sabit + analog valf durumunda daha ekstra yorumlar üretir.
- “Gerçekten kötü” sayılabilecek kombinasyonlarda H.Hit(...) ile rule tetikler.

### on/off valfli PID Doctor:
Aşağıdaki script:
- SP + PV + binary valf (bool) ile çalışır,
- Loop health skorları çıkarır (SSE, stabilite, noise, chatter, duty),
- Bulgular + tuning önerileri üretir,
- Ciddi durumlarda H.Hit(...) ile alarm üretir,
- İstenirse ONNX eğitimi için CSV’ye de feature loglar.

**PID Doctor – On/Off Valfli Loop (bool valf)**
----------------------

```csharp
// =====================================
// PID DOCTOR – On/Off Valve Loop
// =====================================
using System;
using System.Linq;
// ---- 0) KONFİG ----
// Tag'ler
string spTag    = "GVL_Var.fTempSet";
string pvTag    = "GVL_Var.fTempActual";
string valveTag = "GVL_Var.bCompressor";    // bool: true=ON, false=OFF
// Opsiyonel: PID parametre tag'leri (varsa okunur, yoksa sadece davranıştan yorum yapılır)
string kpTag = "GVL_Var.fPID_Kp";
string kiTag = "GVL_Var.fPID_Ki";
string kdTag = "GVL_Var.fPID_Kd";
// Analiz penceresi
int n = 300;
// Örnekleme süresi tahmini (saniye) – sadece bilgi
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
// ---- 2) History çek (SP / PV / Valve) ----
var spHistRaw    = H.LastN(spTag,    n);
var pvHistRaw    = H.LastN(pvTag,    n);
var valveHistRaw = H.LastN(valveTag, n);
if (spHistRaw == null || pvHistRaw == null || valveHistRaw == null)
{
    H.Console($"PID-DOCTOR-ONOFF: history yok. SP={spHistRaw?.Length ?? 0}, PV={pvHistRaw?.Length ?? 0}, VALVE={valveHistRaw?.Length ?? 0}");
    return false;
}
if (spHistRaw.Length == 0 || pvHistRaw.Length == 0 || valveHistRaw.Length == 0)
{
    H.Console($"PID-DOCTOR-ONOFF: boş history. SP={spHistRaw.Length}, PV={pvHistRaw.Length}, VALVE={valveHistRaw.Length}");
    return false;
}
// Üç seriyi hizala + parse edilemeyenleri at
int rawMin = Math.Min(spHistRaw.Length, Math.Min(pvHistRaw.Length, valveHistRaw.Length));
var spList    = new System.Collections.Generic.List<double>();
var pvList    = new System.Collections.Generic.List<double>();
var valveList = new System.Collections.Generic.List<int>();
for (int i = 0; i < rawMin; i++)
{
    var sv = H.Double(spHistRaw[i]);
    var pv = H.Double(pvHistRaw[i]);
    var bv = H.Bool(valveHistRaw[i]);  // bool? → 1/0 yapacağız
    if (sv.HasValue && pv.HasValue && bv.HasValue)
    {
        spList.Add(sv.Value);
        pvList.Add(pv.Value);
        valveList.Add(bv.Value ? 1 : 0);
    }
}
int len = new[] { spList.Count, pvList.Count, valveList.Count }.Min();
if (len < 40)
{
    H.Console($"PID-DOCTOR-ONOFF: Yetersiz ortak örnek (len={len} < 40). SPraw={spHistRaw.Length}, PVraw={pvHistRaw.Length}, VALVEraw={valveHistRaw.Length}");
    return false;
}
// Son n örneği al
if (len > n)
{
    spList    = spList.Skip(len - n).ToList();
    pvList    = pvList.Skip(len - n).ToList();
    valveList = valveList.Skip(len - n).ToList();
    len = n;
}
double[] spVals    = spList.Take(len).ToArray();
double[] pvVals    = pvList.Take(len).ToArray();
int[]    valveVals = valveList.Take(len).ToArray();
// ---- 3) Temel istatistikler ----
double spAvg = spVals.Average();
double pvAvg = pvVals.Average();
// SP std
double spStd;
{
    double m = spAvg;
    double sumSq = spVals.Sum(v => (v - m) * (v - m));
    spStd = Math.Sqrt(sumSq / spVals.Length);
}
// PV std
double pvStd;
{
    double m = pvAvg;
    double sumSq = pvVals.Sum(v => (v - m) * (v - m));
    pvStd = Math.Sqrt(sumSq / pvVals.Length);
}
double spAbs = Math.Max(Math.Abs(spAvg), 1e-6);
// SP stabil mi?
double spStdThresh = Math.Max(spAbs * 0.01, 0.01);   // %1
bool spStable      = spStd <= spStdThresh;
bool spStrongStable = spStd <= Math.Max(spAbs * 0.005, 0.005);  // daha sıkı
// Hata: e = SP - PV
double[] err = new double[len];
for (int i = 0; i < len; i++)
    err[i] = spVals[i] - pvVals[i];
double errAvg     = err.Average();
double errAbsMean = err.Select(Math.Abs).Average();
double errRms     = Math.Sqrt(err.Select(e => e * e).Average());
double errVar = err.Sum(e => (e - errAvg) * (e - errAvg)) / err.Length;
double errStd = Math.Sqrt(errVar);
// Hata işaret değişimleri
int errZeroCross = 0;
for (int i = 1; i < len; i++)
{
    if (Math.Sign(err[i]) != 0 && Math.Sign(err[i - 1]) != 0 &&
        Math.Sign(err[i]) != Math.Sign(err[i - 1]))
        errZeroCross++;
}
// PV max/min
double pvMax = pvVals.Max();
double pvMin = pvVals.Min();
// Overshoot (SP sabitse)
double overshootRatio = 0.0;
if (spStrongStable && spAbs > 1e-6)
{
    overshootRatio = (pvMax - spAvg) / spAbs;
}
// ---- 4) On/Off valf istatistikleri ----
int onCount  = valveVals.Count(v => v == 1);
int offCount = len - onCount;
double dutyOn = onCount / (double)len;
// Toggle (chatter) sayısı
int toggles = 0;
for (int i = 1; i < len; i++)
    if (valveVals[i] != valveVals[i - 1])
        toggles++;
// En uzun ON/OFF koşuları
int longestOnRun = 0, longestOffRun = 0;
{
    int curRun = 1;
    for (int i = 1; i < len; i++)
    {
        if (valveVals[i] == valveVals[i - 1])
        {
            curRun++;
        }
        else
        {
            if (valveVals[i - 1] == 1)
                longestOnRun = Math.Max(longestOnRun, curRun);
            else
                longestOffRun = Math.Max(longestOffRun, curRun);
            curRun = 1;
        }
    }
    // son run
    if (valveVals[len - 1] == 1)
        longestOnRun = Math.Max(longestOnRun, curRun);
    else
        longestOffRun = Math.Max(longestOffRun, curRun);
}
// PV eğimi (ON iken / OFF iken)
double openSlopeSum  = 0.0;
int    openCount     = 0;
double closeSlopeSum = 0.0;
int    closeCount    = 0;
for (int i = 1; i < len; i++)
{
    double dpv = pvVals[i] - pvVals[i - 1];
    if (valveVals[i - 1] == 1)
    {
        openSlopeSum += dpv;
        openCount++;
    }
    else
    {
        closeSlopeSum += dpv;
        closeCount++;
    }
}
double openSlope  = openCount  > 0 ? openSlopeSum  / openCount  : 0.0;
double closeSlope = closeCount > 0 ? closeSlopeSum / closeCount : 0.0;
// ---- 5) Skorlar ----
// 5.1 SSE Score
double sseScore;
{
    double sseRefSmall = Math.Max(spAbs * 0.02, 0.1);
    double sseRefBad   = Math.Max(spAbs * 0.10, 0.5);
    double sseNorm     = Math.Min(errAbsMean / sseRefBad, 1.0);
    sseScore           = (1.0 - sseNorm) * 100.0;
}
// 5.2 Stability / Oscillation Score
double oscIndex = errStd / (Math.Abs(errAvg) + 1e-3);
double oscNorm  = Math.Clamp((oscIndex - 0.5) / (2.0 - 0.5), 0.0, 1.0);
double zeroCrossNorm = Math.Clamp((double)errZeroCross / (len / 5.0), 0.0, 2.0);
zeroCrossNorm = Math.Min(zeroCrossNorm, 1.0);
double stabilityScore = (1.0 - 0.5 * (oscNorm + zeroCrossNorm)) * 100.0;
// 5.3 Noise Score
double noiseScore;
{
    double jitterRatio = pvStd / spAbs;
    double jitterGood  = 0.01;
    double jitterBad   = 0.10;
    double jitterNorm  = (jitterRatio - jitterGood) / (jitterBad - jitterGood);
    jitterNorm         = Math.Clamp(jitterNorm, 0.0, 1.0);
    noiseScore         = (1.0 - jitterNorm) * 100.0;
}
// 5.4 Valve Chatter & Duty Score
double valveChatterScore;
{
    // pencerenin %20'sinden fazla toggle → kötü
    double toggleNorm = Math.Clamp((double)toggles / (len / 5.0), 0.0, 2.0);
    toggleNorm = Math.Min(toggleNorm, 1.0);
    valveChatterScore = (1.0 - toggleNorm) * 100.0;
}
// Duty: çok uzun süre tamamen ON veya OFF ise puanı düşür
double valveDutyScore;
{
    // idealde duty ~0.2–0.8 arası, çok uçlara gidiyorsa kötü
    double dev = Math.Abs(dutyOn - 0.5);      // 0 → iyi, 0.5 → en kötü
    double dutyNorm = Math.Clamp(dev / 0.5, 0.0, 1.0);
    valveDutyScore = (1.0 - dutyNorm) * 100.0;
}
// Genel loop health
double pidPerformance =
    0.4 * sseScore +
    0.3 * stabilityScore +
    0.1 * noiseScore +
    0.1 * valveChatterScore +
    0.1 * valveDutyScore;
// ---- 6) PID Doctor – Bulgular & Öneriler ----
var findings    = new System.Collections.Generic.List<string>();
var suggestions = new System.Collections.Generic.List<string>();
// Genel bulgular
if (!spStable)
    findings.Add("SP belirgin şekilde değişken; on/off loop tuning yorumu sabit SP veya step testi altında daha anlamlı olur.");
if (sseScore < 60.0)
    findings.Add("Kalıcı hata (offset) görece yüksek.");
if (stabilityScore < 60.0)
    findings.Add("Loop kararsız veya belirgin ON/OFF osilasyonları mevcut.");
if (noiseScore < 60.0)
    findings.Add("PV üzerinde belirgin gürültü / jitter var.");
if (toggles > len / 5)
    findings.Add("Valf çok sık aç/kapa yapıyor (chatter).");
if (dutyOn > 0.9)
    findings.Add("Valf çoğunlukla ON (yaklaşık sürekli çalışıyor); kapasite yetersizliği veya loop parametreleri zayıf olabilir.");
else if (dutyOn < 0.1)
    findings.Add("Valf çoğunlukla OFF; SP’ye göre proses fazla güçlü veya setpoint düşük olabilir.");
// SP sabitse ekstra
if (spStrongStable)
{
    if (overshootRatio > 0.3)
        findings.Add($"Step sonrası overshoot çok yüksek (≈ %{overshootRatio * 100.0:F1}).");
    else if (overshootRatio > 0.1)
        findings.Add($"Step sonrası overshoot orta seviyede (≈ %{overshootRatio * 100.0:F1}).");
}
// PV eğimine dayalı proses teşhisi
if (openCount > 0 && openSlope <= 0.0)
    findings.Add("Valf ON iken PV artmıyor (veya düşüyor) → proses yönü, sensör bağlantısı veya P&I tarafında problem olabilir.");
if (closeCount > 0 && closeSlope >= 0.0)
    findings.Add("Valf OFF iken PV düşmüyor → kaçak, büyük rezervuar veya ters akış olabilir.");
// Kp değerlendirme (davranış üzerinden)
if (sseScore < 60.0 && stabilityScore > 75.0 && valveChatterScore > 70.0 && dutyOn < 0.9)
{
    suggestions.Add("Loop yavaş/tembel görünüyor (kalıcı hata yüksek, chatter düşük, duty orta): Kp'yi kademeli (%10–20) artırmayı düşünebilirsin.");
}
else if (stabilityScore < 60.0 || valveChatterScore < 60.0 || toggles > len / 5)
{
    suggestions.Add("Loop osilasyonlu veya valf chatter yapıyor: Kp'yi ve/veya Ki'yi azaltmak, histerezis/debounce eklemek faydalı olabilir.");
}
// Ki değerlendirme
if (integralUsed)
{
    if (sseScore < 60.0 && dutyOn < 0.9 && dutyOn > 0.1)
        suggestions.Add("Kalıcı hata var, duty çok uçlarda değil: integral etkisini (Ki) biraz artırmak offseti azaltabilir.");
    if (dutyOn > 0.9 && sseScore < 80.0)
        suggestions.Add("Valf neredeyse sürekli ON ve hata büyük: integral (Ki) aşırı olabilir veya proses kapasitesi yetersiz; Ki'yi azaltmayı ve anti-windup'u gözden geçirmeyi düşün.");
}
else
{
    if (sseScore < 70.0)
        suggestions.Add("Integral (Ki) devrede değil; kalıcı offseti azaltmak için küçük bir integral bileşeni eklemeyi düşünebilirsin.");
}
// Kd değerlendirme – on/off looplarda genelde yok ama yine de yorum
if (derivativeUsed)
{
    if (noiseScore < 50.0)
        suggestions.Add("PV gürültülü ve türev (Kd) aktif: on/off looplarda Kd kullanımı genelde gürültüyü büyütür; Kd'yi azalt veya tamamen kapatmayı düşünebilirsin.");
}
else
{
    // Genelde iyi; Kd zorunlu değil
}
// Genel loop yorumu
if (pidPerformance >= 80.0 && stabilityScore >= 80.0 && sseScore >= 80.0 && valveChatterScore >= 80.0)
    findings.Add("On/off loop genel olarak sağlıklı görünüyor; yalnızca küçük ince ayarlar gerekebilir.");
else if (pidPerformance < 50.0)
    findings.Add("On/off loop performansı zayıf; temel tuning ve histerezis/debounce ayarlarının gözden geçirilmesi gerekiyor.");
// ---- 7) Konsol raporu ----
H.Console("============================================");
H.Console("PID DOCTOR – On/Off Valve Loop");
H.Console($"SP = {spTag}, PV = {pvTag}, Valve = {valveTag}");
if (kpVal.HasValue || kiVal.HasValue || kdVal.HasValue)
    H.Console($"Kp={kp:F4}, Ki={ki:F4}, Kd={kd:F4}");
else
    H.Console("Kp/Ki/Kd tag'leri tanımlı değil veya okunamadı; davranış üzerinden yorumlanıyor.");
H.Console($"Window: len={len} samples, dt≈{sampleTimeSec:F2}s");
H.Console("--------------------------------------------");
H.Console($"SPavg={spAvg:F3}, SPstd={spStd:F3}  → SP {(spStable ? "stabil" : "oynak")}");
H.Console($"PVavg={pvAvg:F3}, PVstd={pvStd:F3}, PVmax={pvMax:F3}, PVmin={pvMin:F3}");
H.Console($"ErrAvg={errAvg:F3}, |Err|mean={errAbsMean:F3}, ErrRMS={errRms:F3}, ErrStd={errStd:F3}, ZeroCross={errZeroCross}");
if (spStrongStable)
    H.Console($"Approx overshoot ratio ≈ {overshootRatio * 100.0:F1}% (SP sabit varsayımıyla).");
H.Console($"Valve duty ON = {dutyOn * 100.0:F1}%, Toggles = {toggles}, LongestON={longestOnRun}, LongestOFF={longestOffRun}");
H.Console($"PV slope (VALVE ON)  = {openSlope:F4} (unit/sample)");
H.Console($"PV slope (VALVE OFF) = {closeSlope:F4} (unit/sample)");
H.Console("--------------------------------------------");
H.Console("Scores (0-100):");
H.Console($"  Steady-State Error      = {sseScore:F1}");
H.Console($"  Stability/Oscillation   = {stabilityScore:F1}");
H.Console($"  Noise (PV jitter)       = {noiseScore:F1}");
H.Console($"  Valve Chatter           = {valveChatterScore:F1}");
H.Console($"  Valve Duty Balance      = {valveDutyScore:F1}");
H.Console($"  On/Off Loop Performance = {pidPerformance:F1}");
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
H.Console("Öneriler (tuning / mantık yönü):");
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
// ---- 8) (Opsiyonel) ONNX eğitimi için feature log'u ----
// Bunu ONNX modelini eğitirken "feature + label" dataseti olarak kullanabilirsin.
// Şimdilik label kolonunu boş bırakıyoruz (operatör offline doldurabilir).
// Yorumdan çıkararak aktifleştir:
// H.LogSample(
//     "LoopHealth",
//     "pid_doctor_onoff",
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
//     ("PVmax", pvMax),
//     ("PVmin", pvMin),
//     ("OvershootRatio", overshootRatio),
//     ("DutyOn", dutyOn),
//     ("Toggles", toggles),
//     ("LongestOnRun", longestOnRun),
//     ("LongestOffRun", longestOffRun),
//     ("OpenSlope", openSlope),
//     ("CloseSlope", closeSlope),
//     ("SSEScore", sseScore),
//     ("StabilityScore", stabilityScore),
//     ("NoiseScore", noiseScore),
//     ("ValveChatterScore", valveChatterScore),
//     ("ValveDutyScore", valveDutyScore),
//     ("OnOffPerformance", pidPerformance),
//     ("HealthLabel", "")  // ONNX eğitimi için manuel label (A/B/C vs.) ekleyebilirsin
// );
// ---- 9) Alarm (Hit) kararı ----
// On/off loop ciddi sıkıntılıysa H.Hit ile alarm üret.
bool severe =
    pidPerformance < 50.0 ||
    stabilityScore < 50.0 ||
    sseScore < 50.0 ||
    (spStrongStable && overshootRatio > 0.3) ||
    (toggles > len / 4) ||      // aşırı chatter
    (dutyOn > 0.95 && sseScore < 80.0) ||
    (dutyOn < 0.05 && sseScore < 80.0);
if (severe)
{
    return H.Hit("PID Doctor (On/Off): loop ciddi tuning/performans problemi gösteriyor (detay için console/CSV).");
}
// Alarm yok, sadece teşhis
return false;
```
