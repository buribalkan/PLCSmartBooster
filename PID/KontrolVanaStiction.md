# Kontrol Vanası, Pompa Problemleri Teşhis

- #### **Kontrol vanası stiction / ölü bant**
- #### **Kontrol vanası kaçak**
- #### **Pompa performans kaybı (mekanik/impeller vs.)**

> **Kontrol vanası stiction / ölü bant dedektörü**
>    
> **Mantık:**
> 
> - Son N örnekte vanaya çok adım atılmış (ΔCmd büyük),
> 
> - Ancak PV neredeyse kıpırdamıyor (ΔPV çok küçük),
> 
> - Bu durum tekrar ediyorsa “stiction şüphesi”.

```csharp
// =======================================
// CONTROL VALVE STICITION / DEAD-BAND CHECK
// =======================================
using System;
using System.Linq;
// Konfig
string cmdTag = "GVL_Var.fValveCmd";       // 0..100%
string pvTag  = "GVL_Var.fFlowActual";     // veya basınç / sıcaklık
int n = 200;           // analiz penceresi
double minStep = 2.0;  // cmd değişimi [%] altında sayma
double pvEpsRatio = 0.005; // PV aralığının %0.5'i altında ise "kıpırdamıyor" say
// History çek
var cmdStr = H.LastN(cmdTag, n);
var pvStr  = H.LastN(pvTag,  n);
if (cmdStr == null || pvStr == null || cmdStr.Length < 10 || pvStr.Length < 10)
{
    H.Console($"VALVE-STICTION: {cmdTag}/{pvTag} için yeterli veri yok.");
    return false;
}
// Ortak uzunluk
int len = Math.Min(cmdStr.Length, pvStr.Length);
// Double'a çevir + null'ları at
var cmdList = new System.Collections.Generic.List<double>();
var pvList  = new System.Collections.Generic.List<double>();
for (int i = 0; i < len; i++)
{
    var c = H.Double(cmdStr[i]);
    var p = H.Double(pvStr[i]);
    if (c.HasValue && p.HasValue)
    {
        cmdList.Add(c.Value);
        pvList.Add(p.Value);
    }
}
len = Math.Min(cmdList.Count, pvList.Count);
if (len < 10)
{
    H.Console($"VALVE-STICTION: Geçerli ortak örnek sayısı az (len={len}).");
    return false;
}
double[] cmd = cmdList.ToArray();
double[] pv  = pvList.ToArray();
// PV aralığı (normalize için)
double pvMin = pv.Min();
double pvMax = pv.Max();
double pvRange = Math.Max(pvMax - pvMin, 1e-6);
double pvEps   = pvRange * pvEpsRatio;
// adım sayacı
int strongSteps       = 0;
int stepsWithNoEffect = 0;
for (int i = 1; i < len; i++)
{
    double dCmd = cmd[i] - cmd[i - 1];
    double dPv  = pv[i]  - pv[i - 1];
    if (Math.Abs(dCmd) >= minStep)
    {
        strongSteps++;
        if (Math.Abs(dPv) <= pvEps)
            stepsWithNoEffect++;
    }
}
// oran hesap
double ratio = strongSteps > 0 ? (double)stepsWithNoEffect / strongSteps : 0.0;
// eşikler
double minStrongSteps = 5;
double ratioThresh    = 0.5;   // adımların %50'sinde PV kıpırdamıyorsa şüpheli
H.Console("================================");
H.Console($"VALVE STICITION CHECK cmd={cmdTag}, pv={pvTag}");
H.Console($"Samples={len}, StrongSteps={strongSteps}, NoEffectSteps={stepsWithNoEffect}, Ratio={ratio:F2}");
H.Console($"PV Range={pvRange:F3}, pvEps={pvEps:F4}");
bool suspicion =
    strongSteps >= minStrongSteps &&
    ratio >= ratioThresh;
if (suspicion)
{
    H.Console("Yorum: Vanaya adım atılmasına rağmen PV'de yeterli hareket yok → stiction / ölü band şüphesi.");
    // CSV log (isteğe bağlı)
    H.LogSample(
        "MechFault",
        "valve_stiction",
        ("CmdTag", cmdTag),
        ("PVTag",  pvTag),
        ("StrongSteps", strongSteps),
        ("StepsNoEffect", stepsWithNoEffect),
        ("Ratio", ratio),
        ("PVRange", pvRange)
    );
    return H.Hit("Mechanical: Control valve stiction / dead-band suspicion.");
}
else
{
    H.Console("Stiction için belirgin bir bulgu yok.");
    return false;
}
```
2) Kontrol vanası kaçak (closed-valve leakage)
Mantık:
Vanaya “kapat” komutu gidiyor (Cmd < closeLimit),
Ama proses PV’si (örn. debi) uzun süre “sıfıra yakın” değil.
Bu durum pencerede anlamlı bir süre varsa → kaçak şüphesi.

```csharp
// =======================================
// CONTROL VALVE LEAKAGE CHECK (CLOSED BUT FLOW HIGH)
// =======================================
using System;
using System.Linq;
// Konfig
string cmdTag  = "GVL_Var.fValveCmd";       // 0..100%
string flowTag = "GVL_Var.fFlowActual";     // debi tag'i
int n = 300;
double closeLimit  = 5.0;      // %5 altı "kapalı kabul"
double minLeakFlow = 0.05;     // debi range'inin %5'i üstü "kaçağa işaret"
// History
var cmdStr  = H.LastN(cmdTag,  n);
var flowStr = H.LastN(flowTag, n);
if (cmdStr == null || flowStr == null || cmdStr.Length < 20 || flowStr.Length < 20)
{
    H.Console($"VALVE-LEAK: {cmdTag}/{flowTag} için yeterli veri yok.");
    return false;
}
int len = Math.Min(cmdStr.Length, flowStr.Length);
var cmdList  = new System.Collections.Generic.List<double>();
var flowList = new System.Collections.Generic.List<double>();
for (int i = 0; i < len; i++)
{
    var c = H.Double(cmdStr[i]);
    var f = H.Double(flowStr[i]);
    if (c.HasValue && f.HasValue)
    {
        cmdList.Add(c.Value);
        flowList.Add(f.Value);
    }
}
len = Math.Min(cmdList.Count, flowList.Count);
if (len < 20)
{
    H.Console($"VALVE-LEAK: yeterli geçerli örnek yok (len={len}).");
    return false;
}
double[] cmd  = cmdList.ToArray();
double[] flow = flowList.ToArray();
// Flow range
double fMin = flow.Min();
double fMax = flow.Max();
double fRange = Math.Max(fMax - fMin, 1e-6);
double leakFlowThresh = fMin + fRange * minLeakFlow;
// Kapalı sayılan örnekler
int closedCount = 0;
int leakCount   = 0;
for (int i = 0; i < len; i++)
{
    if (cmd[i] <= closeLimit)
    {
        closedCount++;
        if (flow[i] > leakFlowThresh)
            leakCount++;
    }
}
double leakRatio = closedCount > 0 ? (double)leakCount / closedCount : 0.0;
// Eşik: kapalı zamanın en az %30'unda anlamlı debi varsa şüpheli
double minClosedSamples = 30;
double leakRatioThresh  = 0.3;
H.Console("================================");
H.Console($"VALVE LEAK CHECK cmd={cmdTag}, flow={flowTag}");
H.Console($"Samples={len}, ClosedSamples={closedCount}, LeakSamples={leakCount}, LeakRatio={leakRatio:F2}");
H.Console($"FlowRange={fRange:F3}, LeakFlowThresh={leakFlowThresh:F3}");
bool leakSuspicion =
    closedCount >= minClosedSamples &&
    leakRatio >= leakRatioThresh;
if (leakSuspicion)
{
    H.Console("Yorum: Vanaya kapalı komutu verildiği halde debi anlamlı seviyede kalıyor → kaçak şüphesi.");
    H.LogSample(
        "MechFault",
        "valve_leak",
        ("CmdTag", cmdTag),
        ("FlowTag", flowTag),
        ("ClosedSamples", closedCount),
        ("LeakSamples", leakCount),
        ("LeakRatio", leakRatio),
        ("FlowRange", fRange)
    );
    return H.Hit("Mechanical: Control valve leakage suspicion (closed but flow high).");
}
else
{
    H.Console("Valf kaçağına dair belirgin bir sinyal yok.");
    return false;
}
```
3) Pompa performans kaybı (head düşüşü / mekanik arıza)
Varsayım:
runTag  : pompa run command veya feedback (bool)
speedTag: pompa devri (Hz veya rpm)
pDisTag: çıkış basıncı (bar)
Opsiyonel: pSucTag emiş basıncı (NPSH için)
Mantık (basit):
Pompa çalışıyor ve devir yüksek,
Çıkış basıncı, geçmiş “sağlıklı” değerlerin tipik seviyesinin belirgin altında.
Sağlıklı referansı için median/average head’i uzun bir pencereden alıyoruz.

```csharp
// =======================================
// PUMP PERFORMANCE LOSS / MECHANICAL ISSUE
// =======================================
using System;
using System.Linq;
// Konfig
string runTag   = "GVL_Var.bPumpRunFb";      // bool
string speedTag = "GVL_Var.fPumpSpeed";      // Hz veya rpm
string pDisTag  = "GVL_Var.fPumpP_Discharge"; // bar
// string pSucTag  = "GVL_Var.fPumpP_Suction";   // varsa ek analiz yapılabilir
int n = 500;              // uzun pencere
double minSpeed = 30.0;   // bu speed üstü "yük altında" kabul
double minRunRatio = 0.4; // pencerede en az %40 çalışma olsun
// History
var runStr   = H.LastN(runTag,   n);
var speedStr = H.LastN(speedTag, n);
var pDisStr  = H.LastN(pDisTag,  n);
if (runStr == null || speedStr == null || pDisStr == null)
{
    H.Console("PUMP-HEALTH: history yok.");
    return false;
}
int len = new[] { runStr.Length, speedStr.Length, pDisStr.Length }.Min();
if (len < 50)
{
    H.Console($"PUMP-HEALTH: yeterli örnek yok (len={len}).");
    return false;
}
var speedList = new System.Collections.Generic.List<double>();
var pDisList  = new System.Collections.Generic.List<double>();
var runList   = new System.Collections.Generic.List<bool>();
for (int i = 0; i < len; i++)
{
    var r = H.Bool(runStr[i]);
    var s = H.Double(speedStr[i]);
    var p = H.Double(pDisStr[i]);
    if (r.HasValue && s.HasValue && p.HasValue)
    {
        runList.Add(r.Value);
        speedList.Add(s.Value);
        pDisList.Add(p.Value);
    }
}
len = new[] { runList.Count, speedList.Count, pDisList.Count }.Min();
if (len < 50)
{
    H.Console($"PUMP-HEALTH: geçerli örnek yetersiz (len={len}).");
    return false;
}
bool[]   run   = runList.Take(len).ToArray();
double[] speed = speedList.Take(len).ToArray();
double[] pDis  = pDisList.Take(len).ToArray();
// Çalışma oranı
int runSamples = run.Count(b => b);
double runRatio = runSamples / (double)len;
if (runRatio < minRunRatio)
{
    H.Console($"PUMP-HEALTH: Pencerenin çoğunda pompa OFF (RunRatio={runRatio:F2}). Analiz atlandı.");
    return false;
}
// Çalışma anlarındaki "head" (discharge pressure) istatistiği
var runningPressures = pDis
    .Zip(run, (p, r) => (p, r))
    .Where(x => x.r)
    .Select(x => x.p)
    .ToArray();
var runningSpeeds = speed
    .Zip(run, (s, r) => (s, r))
    .Where(x => x.r)
    .Select(x => x.s)
    .ToArray();
if (runningPressures.Length < 30)
{
    H.Console("PUMP-HEALTH: Çalışma halinde yeterli örnek yok.");
    return false;
}
double pAvg = runningPressures.Average();
// Referans: "yüksek devir" anlarındaki tipik basınç
var highLoadPressures = runningPressures
    .Zip(runningSpeeds, (p, s) => (p, s))
    .Where(x => x.s >= minSpeed)
    .Select(x => x.p)
    .ToArray();
if (highLoadPressures.Length < 10)
{
    H.Console("PUMP-HEALTH: Yüksek devirde yeterli örnek yok.");
    return false;
}
// Median fonksiyonu
double Median(double[] xs)
{
    Array.Sort(xs);
    int m = xs.Length;
    return (m % 2 == 1) ? xs[m / 2] : 0.5 * (xs[m / 2 - 1] + xs[m / 2]);
}
double pHighMed = Median(highLoadPressures);
// Şu anki durum: son örneklerdeki basınca bakalım
int lastWindow = Math.Min(50, highLoadPressures.Length);
double pLastMed = Median(highLoadPressures.Skip(highLoadPressures.Length - lastWindow).ToArray());
// Degrade oranı
double drop = pHighMed - pLastMed;
double dropRatio = pHighMed > 1e-6 ? drop / pHighMed : 0.0;
// Eşik: head %30’dan fazla düşmüşse mekanik arıza şüphesi
double dropRatioThresh = 0.3;
H.Console("================================");
H.Console($"PUMP HEALTH run={runTag}, speed={speedTag}, pDis={pDisTag}");
H.Console($"RunRatio={runRatio:F2}, HighLoadSamples={highLoadPressures.Length}");
H.Console($"HeadBaseMed={pHighMed:F2}, HeadRecentMed={pLastMed:F2}, DropRatio={dropRatio * 100.0:F1}%");
bool suspect = dropRatio >= dropRatioThresh;
if (suspect)
{
    H.Console("Yorum: Aynı devirlerde eskiye göre çıkış basıncı belirgin düşük → impeller aşınması, kaçak, mekanik arıza şüphesi.");
    H.LogSample(
        "MechFault",
        "pump_head_loss",
        ("RunTag", runTag),
        ("SpeedTag", speedTag),
        ("PDisTag", pDisTag),
        ("RunRatio", runRatio),
        ("BaseHeadMed", pHighMed),
        ("RecentHeadMed", pLastMed),
        ("DropRatio", dropRatio)
    );
    return H.Hit("Mechanical: Pump head loss / mechanical degradation suspicion.");
}
else
{
    H.Console("Pompa için belirgin head kaybı tespit edilmedi.");
    return false;
}
```
