# Machine Learning Examples Including PLC Code

###  SR - CNN tek değişkende ani anomali yakalama
Mantık: kayan pencerede mean/std → son değere z-skor → |z| > K olunca (ve önce değilse) HIT.

```CSharp
// ==== SR-CNN (approx) : tek tag ====
static readonly int Win = 128;
static readonly double K = 3.0;      // z-threshold
static readonly Queue<double> Q = new();
static bool lastAnom = false;
double? v = Event.ValueAsDouble(); // bu kuralı tek tag için kullan: Include ile filtrele
if (v == null) return false;
Q.Enqueue(v.Value);
while (Q.Count > Win) Q.Dequeue();
if (Q.Count < Math.Min(Win, 16)) { H.Console($"[SR] warmup {Q.Count}/{Win}"); return false; }
// mean/std
double sum = 0, sum2 = 0; foreach (var x in Q) { sum += x; sum2 += x * x; }
int n = Q.Count;
double mean = sum / n;
double var = Math.Max(0.0, sum2 / n - mean * mean);
double std = Math.Sqrt(var);
double z = (std > 1e-12) ? Math.Abs((v.Value - mean) / std) : 0.0;
H.Console($"[SR] n={n} mean={mean:0.###} std={std:0.###} v={v:0.###} z={z:0.###} thr={K}");
bool anom = z > K;
if (anom && !lastAnom)
{
    return H.Hit($"SR-approx anomaly z={z:0.##} > {K}, v={v:0.###}, μ={mean:0.###}, σ={std:0.###}");
}
lastAnom = anom;
return false;
```

### 2) SSA - Spike tek değişkende “sıçrama” yakalama
Mantık: kısa / uzun pencere ortalaması(trend) +kısa pencere std(gürültü) → raw = | v - trend |, z = raw / std; “p - değeri” ni de yaklaşık gösteriyoruz. alert = z > K.
// ==== SSA-Spike (approx) : tek tag ====

```CSharp
static readonly int ShortW = 32;   // kısa pencere
static readonly int LongW = 192;  // trend penceresi
static readonly double K = 3.0;    // z eşiği
static readonly Queue<double> QS = new();
static readonly Queue<double> QL = new();
static bool lastAlert = false;
double? v = Event.ValueAsDouble();
if (v == null) return false;
QS.Enqueue(v.Value); while (QS.Count > ShortW) QS.Dequeue();
QL.Enqueue(v.Value); while (QL.Count > LongW) QL.Dequeue();
if (QS.Count < 8 || QL.Count < 2 * ShortW) { H.Console($"[SSA] warmup s={QS.Count}/{ShortW}, l={QL.Count}/{LongW}"); return false; }
// mean/std short
double sSum = 0, sSum2 = 0; foreach (var x in QS) { sSum += x; sSum2 += x * x; }
int ns = QS.Count; double sMean = sSum / ns; double sVar = Math.Max(0.0, sSum2 / ns - sMean * sMean);
double sStd = Math.Sqrt(sVar);
// trend: long mean
double lSum = 0; foreach (var x in QL) lSum += x;
int nl = QL.Count; double lMean = lSum / nl;
double raw = Math.Abs(v.Value - lMean);
double z = (sStd > 1e-12) ? raw / sStd : 0.0;
// "p-value" ~ 2*(1-Phi(z)) ~ exp(-z^2/2) kaba approx (sadece gösterim)
double p = Math.Exp(-0.5 * z * z);
bool alert = z > K;
H.Console($"[SSA] v={v:0.###} trend={lMean:0.###} raw={raw:0.###} z={z:0.###} p~{p:0.###} alert={(alert ? 1 : 0)}");
if (alert && !lastAlert)
{
    return H.Hit($"SSA-approx spike z={z:0.##} > {K}, p~{p:0.###}");
}
lastAlert = alert;
return false;
```

### 3) PCA(3 tag) — adım adım log + anomali
3 özelliği z-skorla standardize → 3×3 kovaryans → Jacobi ile eigen → PC1/PC2 çıkart →
son noktayı projekte et → PC1^2+PC2^2 büyükse anomali.
🔧 Aşağıdaki üç tag adını PLC gerçek tag adları ile değiştir:
"MotorCurrent", "Torque", "Temperature"

```CSharp
// ==== PCA (3-feature) step trace & anomaly ====
static readonly string[] FEATS = new[] { "MotorCurrent", "Torque", "Temperature" };
static readonly int W = 200;
static readonly Dictionary<string, Queue<double>> Buf = new(StringComparer.OrdinalIgnoreCase){
    {FEATS[0], new Queue<double>()},
    {FEATS[1], new Queue<double>()},
    {FEATS[2], new Queue<double>()},
};
static readonly double RadiusThr = 3.0; // PC1^2 + PC2^2 eşiği (z-uzayında)
bool ok = true;
foreach (var f in FEATS)
{
    var dv = H.Double(f);
    if (dv.HasValue)
    {
        var q = Buf[f];
        q.Enqueue(dv.Value);
        while (q.Count > W) q.Dequeue();
    }
    else
    {
        ok = false;
    }
}
if (!ok || Buf.Values.Any(q => q.Count < 16))
{
    H.Console($"[PCA] warmup: {string.Join(", ", FEATS.Select(f => $"{f}:{Buf[f].Count}"))}");
    return false;
}
// hizala: en kısa kuyruk
int n = Buf.Values.Select(q => q.Count).Min();
double[,] A = new double[n, 3];
for (int j = 0; j < 3; j++)
{
    var vals = Buf[FEATS[j]].Skip(Buf[FEATS[j]].Count - n).ToArray();
    for (int i = 0; i < n; i++) A[i, j] = vals[i];
}
// standardize
double[] mean = new double[3], std = new double[3];
for (int j = 0; j < 3; j++)
{
    double m = 0; for (int i = 0; i < n; i++) m += A[i, j]; m /= n; mean[j] = m;
    double s2 = 0; for (int i = 0; i < n; i++) { double d = A[i, j] - m; s2 += d * d; }
    double s = Math.Sqrt(s2 / Math.Max(1, n - 1)); std[j] = (s > 1e-12) ? s : 1.0;
    for (int i = 0; i < n; i++) A[i, j] = (A[i, j] - m) / std[j];
}
// kovaryans (z-skorlarıyla ~ korelasyon)
double[,] C = new double[3, 3];
for (int a = 0; a < 3; a++)
    for (int b = a; b < 3; b++)
    {
        double sum = 0; for (int i = 0; i < n; i++) sum += A[i, a] * A[i, b];
        double v = sum / Math.Max(1, n - 1);
        C[a, b] = C[b, a] = v;
    }
// Jacobi eigen (3x3 simetrik)
(double[] lam, double[,] V) = Jacobi(C);
// sırala
int[] idx = new[] { 0, 1, 2 }.OrderByDescending(k => lam[k]).ToArray();
double[] L = idx.Select(k => lam[k]).ToArray();
double[,] E = new double[3, 3];
for (int j = 0; j < 3; j++) for (int i = 0; i < 3; i++) E[i, j] = V[i, idx[j]]; // PC sütunları
double sumL = Math.Max(1e-12, L.Sum());
double ev1 = L[0] / sumL, ev2 = L[1] / sumL, ev3 = L[2] / sumL;
// son nokta projeksiyonu
double x0 = A[n - 1, 0], x1 = A[n - 1, 1], x2 = A[n - 1, 2];
double pc1 = x0 * E[0, 0] + x1 * E[1, 0] + x2 * E[2, 0];
double pc2 = x0 * E[0, 1] + x1 * E[1, 1] + x2 * E[2, 1];
double r2 = pc1 * pc1 + pc2 * pc2;
// log
H.Console("—— PCA ——");
H.Console($"N={n}");
H.Console($"Means: [{mean[0]:0.###}, {mean[1]:0.###}, {mean[2]:0.###}]");
H.Console($"Stds : [{std[0]:0.###}, {std[1]:0.###}, {std[2]:0.###}]");
H.Console($"Cov  : [{C[0, 0]:0.###} {C[0, 1]:0.###} {C[0, 2]:0.###}]");
H.Console($"       [{C[1, 0]:0.###} {C[1, 1]:0.###} {C[1, 2]:0.###}]");
H.Console($"       [{C[2, 0]:0.###} {C[2, 1]:0.###} {C[2, 2]:0.###}]");
H.Console($"Eigen (sorted): [{L[0]:0.###}, {L[1]:0.###}, {L[2]:0.###}]");
H.Console($"ExplainedVar : PC1={ev1:P1} PC2={ev2:P1} PC3={ev3:P1}");
H.Console($"PC1 vec: [{E[0, 0]:0.###}, {E[1, 0]:0.###}, {E[2, 0]:0.###}]");
H.Console($"PC2 vec: [{E[0, 1]:0.###}, {E[1, 1]:0.###}, {E[2, 1]:0.###}]");
H.Console($"Last -> (PC1,PC2)=({pc1:0.###}, {pc2:0.###}), r²={r2:0.###}, thr={RadiusThr:0.###}");
if (r2 > RadiusThr * RadiusThr)
{
    return H.Hit($"PCA anomaly: r={Math.Sqrt(r2):0.##} > {RadiusThr} (PC1={pc1:0.##}, PC2={pc2:0.##})");
}
return false;
// --- Küçük Jacobi (3x3 simetrik) ---
static (double[] evals, double[,] evecs) Jacobi(double[,] A)
{
    double[,] a = new double[3, 3];
    for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) a[i, j] = A[i, j];
    double[,] v = new double[3, 3];
    for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) v[i, j] = (i == j) ? 1 : 0;
    for (int it = 0; it < 30; it++)
    {
        int p = 0, q = 1; double max = Math.Abs(a[0, 1]);
        if (Math.Abs(a[0, 2]) > max) { max = Math.Abs(a[0, 2]); p = 0; q = 2; }
        if (Math.Abs(a[1, 2]) > max) { max = Math.Abs(a[1, 2]); p = 1; q = 2; }
        if (max < 1e-10) break;
        double app = a[p, p], aqq = a[q, q], apq = a[p, q];
        double phi = 0.5 * Math.Atan2(2 * apq, aqq - app);
        double c = Math.Cos(phi), s = Math.Sin(phi);
        for (int k = 0; k < 3; k++) { double aip = a[p, k], aiq = a[q, k]; a[p, k] = c * aip - s * aiq; a[q, k] = s * aip + c * aiq; }
        for (int k = 0; k < 3; k++) { double apk = a[k, p], aqk = a[k, q]; a[k, p] = c * apk - s * aqk; a[k, q] = s * apk + c * aqk; }
        for (int k = 0; k < 3; k++) { double vkp = v[k, p], vkq = v[k, q]; v[k, p] = c * vkp - s * vkq; v[k, q] = s * vkp + c * vkq; }
    }
    return (new[] { a[0, 0], a[1, 1], a[2, 2] }, v);
}
```

Notlar
Include/Exclude filtreleri Rule tarafında kullanılıyor; bu scriptler tek bir tag (SR/SSA) veya 3 tag (PCA) 
için tasarlandı. PCA scriptinde FEATS dizisini kullanılacak tag’lerle değiştirmek gerekir.
H.Console(...) her adımda detay bildirir; “Console” tabında canlı görülür.
Hepsi rising-edge davranır: anomali ilk kez yükselince HIT üretir, sürekli yağdırmaz.



#### Mode = 1 → SR - CNN (tek değişken, bazen sert z - spike)
#### Mode = 2 → SSA - Spike (tek değişken, trend üstüne kısa süreli sıçrama)
#### Mode = 3 → PCA - Normal(3 değişken arası güçlü korelasyon)
#### Mode = 4 → PCA - Anomali(korelasyonu belirli aralıklarla boz)

Aşağıdaki isimlerle TwinCAT tag’ları oluşacak:
GVL.MotorCurrent, 
GVL.Torque, 
GVL.Temperature, 
GVL.TestSignal, 
GVL.Mode, 
GVL.Enable.
(Rule tarafında Include’ı GVL.* şeklinde veya tam adlarla eşleştirilebilinir.)

1) GVL – Global Declarations

```Pascal
{attribute 'qualified_only'}
VAR_GLOBAL
    // Sim kontrol
    Enable         : BOOL:= TRUE;   // PRG sim'i çalıştırsın mı
Mode: INT:= 3;       // 1:SR, 2:SSA, 3:PCA-OK, 4:PCA-Anom
CycleMs: UDINT:= 20;    // task period (ms) -> isteğe göre değiştir
// Gözlenecek taglar
MotorCurrent: REAL:= 10.0;   // A
Torque: REAL:= 20.0;   // B
Temperature: REAL:= 30.0;   // C
TestSignal: REAL:= 0.0;    // SR/SSA tek değişken
// İç durum
_t: LREAL:= 0.0;   // zaman (s)
_seed: UDINT:= 123456789; // RNG tohum
END_VAR
```
Not: TwinCAT’te sembol adları GVL.MotorCurrent gibi çıkacaktır. Scriptlerde FEATS’i buna göre ver ya da Include’a GVL.* koy.
3) Yardımcı: basit RNG + gürültü

```Pascal
// 0..1 arası uniform (LCG)
FUNCTION F_RandU01 : LREAL
VAR_INPUT
    pSeed : REFERENCE TO UDINT;
END_VAR
VAR
    a : UDINT:= 1664525;
c: UDINT:= 1013904223;
x: UDINT;
END_VAR
x := a * pSeed + c;
pSeed:= x;
F_RandU01:= LREAL(x) / LREAL(16#FFFFFFFF);
// ~N(0,1) yaklaşık (12 uniformun toplamı - 6)  => "Box-Muller" yerine hafif yöntem
FUNCTION F_RandN01: LREAL
VAR_INPUT
pSeed: REFERENCE TO UDINT;
END_VAR
VAR
    i : INT;
s: LREAL:= 0.0;
END_VAR
FOR i := 1 TO 12 DO
    s := s + F_RandU01(pSeed:= pSeed);
END_FOR
F_RandN01 := s - 6.0;

```
4) PRG_AnomalySim – ana simülasyon programı
```Pascal
PROGRAM PRG_AnomalySim
VAR
    dt          : LREAL;        // saniye
tSpike: TON;          // spike pencereleri
tGap: TON;          // periyodik tetikleyici
tModeFlip: TON;          // PCA anomaly aralığı
doSpike: BOOL;
doAnom: BOOL;
baseTrend: LREAL;
u: LREAL;        // uniform
n: LREAL;        // normal(0,1)
// parametreler
// SR-CNN-ish
sr_ZBaseAmp: LREAL:= 0.2;     // temel gürültü std
sr_SpikeAmp: LREAL:= 6.0;     // spike şiddeti (z~3 üstü hedef)
sr_Period: TIME:= T#7s;     // spike periyodu
    sr_Width: TIME:= T#300ms;  // spike süresi
    // SSA-ish
    ssa_TrendAmp: LREAL:= 0.01;    // yavaş trend salınımı
ssa_Noise: LREAL:= 0.15;
ssa_Spike: LREAL:= 3.5;     // kısa sıçrama (z ~ 3..4)
ssa_Period: TIME:= T#10s;
    ssa_Width: TIME:= T#400ms;
    // PCA
    pca_NoiseCur: LREAL:= 0.3;
pca_NoiseTq: LREAL:= 0.5;
pca_NoiseTmp: LREAL:= 0.2;
pca_kTq: LREAL:= 2.2;     // Torque ≈ k * Current + noise
pca_kTmp: LREAL:= 0.25;    // Temp ≈ 25 + k*Current + noise
pca_BaseTmp: LREAL:= 25.0;
pca_AnomDur: TIME:= T#8s;     // korelasyon bozulma süresi
    pca_AnomGap: TIME:= T#22s;    // arada normal dönemi
END_VAR
// --- zaman adımı ---
dt := LREAL(GVL.CycleMs) * 0.001;
IF GVL.Enable THEN
    GVL._t := GVL._t + dt;
END_IF
// --- rng örnekleri (her döngü) ---
u := F_RandU01(pSeed:= GVL._seed);
n:= F_RandN01(pSeed:= GVL._seed);
IF NOT GVL.Enable THEN
    RETURN;
END_IF
CASE GVL.Mode OF
// ======================================================
// 1) SR-CNN-ish : tek değişken TestSignal, bazen sert spike
// ======================================================
1:
    // baz çizgi: küçük gürültü + hafif sinüs
    baseTrend:= 10.0 + 0.8 * SIN(2.0 * 3.14159 * 0.05 * GVL._t); // ~20s periyot
GVL.TestSignal := REAL(baseTrend + sr_ZBaseAmp * n);
// periyodik spike penceresi
tGap(IN:= TRUE, PT:= sr_Period);
IF tGap.Q THEN
        tGap(IN := FALSE);
doSpike:= TRUE;
tSpike(IN:= TRUE, PT:= sr_Width);
END_IF
IF doSpike THEN
        IF tSpike.Q THEN
            // spike bitti
            tSpike(IN := FALSE);
doSpike:= FALSE;
ELSE
            // spike anında ani sıçrama (pozitif ya da negatif)
            GVL.TestSignal := GVL.TestSignal + REAL(sr_SpikeAmp * (SEL(u > 0.5, -1.0, 1.0)));
END_IF
END_IF
// ======================================================
// 2) SSA-Spike-ish : trend + kısa spike (tek değişken)
// ======================================================
2:
    // yavaş trend + gürültü
    baseTrend:= 12.0 + ssa_TrendAmp * SIN(2.0 * 3.14159 * 0.01 * GVL._t);
GVL.TestSignal := REAL(baseTrend + ssa_Noise * n);
// periyodik kısa spike
tGap(IN:= TRUE, PT:= ssa_Period);
IF tGap.Q THEN
        tGap(IN := FALSE);
doSpike:= TRUE;
tSpike(IN:= TRUE, PT:= ssa_Width);
END_IF
IF doSpike THEN
        IF tSpike.Q THEN
            tSpike(IN := FALSE);
doSpike:= FALSE;
ELSE
            // trend etrafına mutlak sapma yarat
            GVL.TestSignal := GVL.TestSignal + REAL(ssa_Spike * (SEL(u > 0.5, -1.0, 1.0)));
END_IF
END_IF
// ======================================================
// 3) PCA-Normal : 3 değişken arası iyi korelasyon
// ======================================================
3:
    // Current: baz + yavaş salınım + küçük gürültü
    GVL.MotorCurrent := REAL(8.0 + 2.5 * SIN(2.0 * 3.14159 * 0.02 * GVL._t) + pca_NoiseCur * n);
// Torque ≈ k * Current + noise
GVL.Torque := REAL(pca_kTq * LREAL(GVL.MotorCurrent) + pca_NoiseTq * F_RandN01(pSeed:= GVL._seed));
// Temperature ≈ base + k * Current + noise
GVL.Temperature := REAL(pca_BaseTmp + pca_kTmp * LREAL(GVL.MotorCurrent) + pca_NoiseTmp * F_RandN01(pSeed:= GVL._seed));
// ======================================================
// 4) PCA-Anomali : çoğu zaman normal, arada korelasyon bozulur
// ======================================================
4:
    // Normal faz
    tModeFlip(IN:= TRUE, PT:= pca_AnomGap);
IF tModeFlip.Q THEN
        // Anomali fazını başlat
        tModeFlip(IN := FALSE);
doAnom:= TRUE;
tSpike(IN:= TRUE, PT:= pca_AnomDur); // anomaly window reuse
END_IF
    // Current her iki fazda da benzer üretelim
    GVL.MotorCurrent := REAL(9.0 + 2.2 * SIN(2.0 * 3.14159 * 0.018 * GVL._t) + pca_NoiseCur * n);
IF doAnom THEN
        // ANOMALİ FAZI: ilişkiyi boz → Torque’u ters fazlı/offsetli yap,
        // Temp’i de akımdan bağımsız “salt noise + offset” gibi dağıt
        GVL.Torque := REAL(-1.6 * LREAL(GVL.MotorCurrent) + 8.0 + 2.0 * F_RandN01(pSeed:= GVL._seed));
GVL.Temperature := REAL(pca_BaseTmp + 5.0 * SIN(2.0 * 3.14159 * 0.35 * GVL._t)
                         + 1.2 * F_RandN01(pSeed:= GVL._seed));
IF tSpike.Q THEN
            // anomaly bitti
            tSpike(IN := FALSE);
doAnom:= FALSE;
// gap zamanlayıcıyı yeniden başlat
tModeFlip(IN:= TRUE, PT:= pca_AnomGap);
END_IF
ELSE
        // NORMAL FAZ
        GVL.Torque := REAL(pca_kTq * LREAL(GVL.MotorCurrent) + pca_NoiseTq * F_RandN01(pSeed:= GVL._seed));
GVL.Temperature := REAL(pca_BaseTmp + pca_kTmp * LREAL(GVL.MotorCurrent) + pca_NoiseTmp * F_RandN01(pSeed:= GVL._seed));
END_IF
ELSE
    // Mode=0 veya bilinmeyen → hiçbir şey yapma / sabitle
END_CASE

```


#### SR-CNN script (tek değişken) için:
##### Mode=1 yapılıp → GVL.TestSignal’ı Include edilecek. (GVL.TestSignal ya da GVL.*), threshold K≈3.
##### Console’da “warmup” sonrası spike pencerelerinde HIT beklenir.

#### SSA-Spike script (tek değişken + trend) için:
##### Mode=2 yapılıp → yine GVL.TestSignal’ı izlenecek. Trend var; kısa genişlikte spike’lar üretir.

#### PCA script (3 değişken) için:
##### Mode=3 ile normal korelasyon (genelde HIT yok).
##### Mode=4 ile her ~22 saniyede 8 saniyelik anomaly penceresi; bu sırada PCA r² büyüyecek ve HIT görülecek.
##### Scriptteki FEATS dizisinin TwinCAT sembollerine göre ayarlanması gerekir:


```Pascal
static readonly string[] FEATS = new[] {

    "GVL.MotorCurrent", "GVL.Torque", "GVL.Temperature"

};

```
Rule Include: GVL.* yapılması halinde herbir tagi yazmaya gerek kalmaz.
