//TAG_WEIGHT ve TAG_BARCODE değerlerini sistemindeki gerçek isimlerle eşleştir.
//SPEC sözlüğünü barkod → (nominal, tolerans) doldur.
//Z-score modu için:
//SAME_DIR=true → hepsi aynı yönde (tamamı pozitif ya da tamamı negatif sapma).
//SAME_DIR=false → çoğunluk (en az MIN_STRONG ölçümde |z|>K).

//PER_BARCODE_COOLDOWN ile barkoda özgü “hit tekrarını” boğarsın; global CooldownMs ile birlikte spam’ı azaltır.
//Tek pencere her barkod için ayrı tutulur; barkod değişince otomatik yeni pencereye yazmaya başlar.




//parametreleri değiştirerek spec (nominal±tol), z-score ya da ikisi birden çalıştır
// ====== USING'ler (gerekirse) ======
using System;
using System.Linq;
using System.Collections.Generic;
// KaraKutu tarafını host zaten ekliyor: using KaraKutu.Core; using KaraKutu.Core.Rules;
// ====== KULLANICI PARAMETRELERİ ======
// Hangi trend modları çalışsın? "spec", "zscore", "both"
const string MODE = "both";
// Etiket adları (adres içinde geçiyorsa eşleşir)
const string TAG_WEIGHT = "MAIN_Simu.fScaleWeight";  // Ölçüm (ağırlık) tag'i
const string TAG_BARCODE = "MAIN_Simu.sBarcode";      // Barkod tag'i
// Pencere ve eşikler
const int WINDOW = 20;      // Son N ölçüm
const int MIN_SAMPLES = 20;      // Trend hesaplamak için min örnek
const double K_SIGMA = 2.5;     // z-score eşiği
const bool SAME_DIR = true;    // z-score: aynı yönde mi şartı (hepsi + veya hepsi -)
const int MIN_STRONG = 20;      // z-score: |z|>K olan en az kaç örnek? (SAME_DIR=false ise çoğunluk eşiği)
// Spec (nominal±tol) için: en az kaç örnek tolerans dışı olursa?
const int MIN_OUT_OF_TOL = 20;     // "tamamı" = 20; "çoğunluk" = 15 gibi
// Barkod ile ölçüm arasındaki max süre (eşleşmenin “taze” sayılması)
var BARCODE_MAX_AGE = TimeSpan.FromSeconds(10);
// İsteğe bağlı (barkoda özel) cooldown (Rule'un CooldownMs'ine ek throttling)
var PER_BARCODE_COOLDOWN = TimeSpan.FromSeconds(5);
// Barkod → (nominal, tolerans)
static readonly Dictionary<string, (double nominal, double tol)> SPEC
    = new(StringComparer.OrdinalIgnoreCase)
    {
        // ÖRNEKLER — kendi barkodlarına göre doldur:
        ["BRD-ABC"] = (500.0, 10.0),
        ["BRD-XYZ"] = (350.0, 7.0),
    };
// ====== KALICI DURUM ======
static string? s_currentBarcode = null;
static DateTime s_currentBarcodeTs = DateTime.MinValue;
static readonly Dictionary<string, List<double>> s_byBarcode = new(StringComparer.OrdinalIgnoreCase);
static readonly Dictionary<string, DateTime> s_lastHitUtcByBarcode = new(StringComparer.OrdinalIgnoreCase);
// ====== YARDIMCI ======
bool IsWeight(string a) => a?.IndexOf(TAG_WEIGHT, StringComparison.OrdinalIgnoreCase) >= 0;
bool IsBarcode(string a) => a?.IndexOf(TAG_BARCODE, StringComparison.OrdinalIgnoreCase) >= 0;
void PushSample(string key, double v)
{
    if (!s_byBarcode.TryGetValue(key, out var list))
        s_byBarcode[key] = list = new List<double>(WINDOW);
    list.Add(v);
    if (list.Count > WINDOW) list.RemoveAt(0);
}
double Mean(IReadOnlyList<double> xs) => xs.Count == 0 ? 0.0 : xs.Average();
double Std(IReadOnlyList<double> xs)
{
    if (xs.Count < 2) return 0.0;
    var m = Mean(xs);
    var varPop = 0.0;
    for (int i = 0; i < xs.Count; i++) { var d = xs[i] - m; varPop += d * d; }
    varPop /= (xs.Count - 1);
    return Math.Sqrt(varPop);
}
bool PerBarcodeCooldownOk(string barcode)
{
    if (PER_BARCODE_COOLDOWN <= TimeSpan.Zero) return true;
    var now = DateTime.UtcNow;
    if (!s_lastHitUtcByBarcode.TryGetValue(barcode, out var last)) return true;
    return (now - last) >= PER_BARCODE_COOLDOWN;
}
void MarkBarcodeHit(string barcode) => s_lastHitUtcByBarcode[barcode] = DateTime.UtcNow;
// ====== AKIŞ ======
var addr = Event.Address ?? "";
// 1) Barkod güncelle
if (IsBarcode(addr))
{
    var s = Event.Value?.ToString();
    if (!string.IsNullOrWhiteSpace(s))
    {
        s_currentBarcode = s.Trim();
        s_currentBarcodeTs = DateTime.UtcNow;
    }
    return false;
}
// 2) Ağırlık verisi geldiyse pencereye koy ve trendleri değerlendir
if (!IsWeight(addr)) return false;
var v = Event.ValueAsDouble();
if (!v.HasValue) return false;
if (string.IsNullOrWhiteSpace(s_currentBarcode)) return false;
if ((DateTime.UtcNow - s_currentBarcodeTs) > BARCODE_MAX_AGE) return false;
var bc = s_currentBarcode!;
PushSample(bc, v.Value);
if (!s_byBarcode.TryGetValue(bc, out var list) || list.Count < Math.Max(1, Math.Min(WINDOW, MIN_SAMPLES)))
    return false;
// 2.a) MODE: spec (nominal ± tol)
bool CheckSpec(out string? reasonSpec)
{
    reasonSpec = null;
    if (!SPEC.TryGetValue(bc, out var sp)) return false; // bilinmeyen barkod: bu modu atla
    int outCnt = list.Count(x => Math.Abs(x - sp.nominal) > sp.tol);
    if (outCnt >= MIN_OUT_OF_TOL)
    {
        reasonSpec = $"[SPEC] Barkod={bc}: son {list.Count} ölçümün {outCnt} tanesi tolerans dışı. " +
                     $"(μ*={sp.nominal:F1}, tol=±{sp.tol:F1})";
        return true;
    }
    return false;
}
// 2.b) MODE: zscore
bool CheckZScore(out string? reasonZ)
{
    reasonZ = null;
    if (list.Count < MIN_SAMPLES) return false;
    var mu = Mean(list);
    var sig = Std(list);
    if (sig <= 0) return false;
    int posStrong = list.Count(x => (x - mu) / sig > +K_SIGMA);
    int negStrong = list.Count(x => (x - mu) / sig < -K_SIGMA);
    bool pass;
    if (SAME_DIR)
        pass = (posStrong == list.Count) || (negStrong == list.Count);
    else
        pass = (posStrong + negStrong) >= MIN_STRONG;
    if (pass)
    {
        var dir = (posStrong > negStrong) ? "pozitif" : (negStrong > posStrong) ? "negatif" : "karışık";
        reasonZ = $"[Z] Barkod={bc}: {dir} trend. |z|>{K_SIGMA}, μ={mu:F2}, σ={sig:F2}, +={posStrong}, -={negStrong}, n={list.Count}";
        return true;
    }
    return false;
}
// 3) Seçilen moda göre değerlendir ve hit ver
bool wantSpec = MODE.Equals("spec", StringComparison.OrdinalIgnoreCase) || MODE.Equals("both", StringComparison.OrdinalIgnoreCase);
bool wantZScore = MODE.Equals("zscore", StringComparison.OrdinalIgnoreCase) || MODE.Equals("both", StringComparison.OrdinalIgnoreCase);
string? reason = null;
bool specOk = false, zOk = false;
if (wantSpec) specOk = CheckSpec(out var r1) && (reason = r1) != null;
if (wantZScore) zOk = CheckZScore(out var r2) && (reason = r2) != null;
if ((specOk || zOk) && PerBarcodeCooldownOk(bc))
{
    MarkBarcodeHit(bc);
    return H.Hit(reason ?? "Trend hit");
}
return false;
