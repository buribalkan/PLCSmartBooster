//İkisi de barkoda göre son 20 ölçümü pencere olarak tutuyor ve trend koşulu sağlanırsa H.Hit(...) ile alarm veriyor. (Rule’un CooldownMs’i spami engeller.)
//1) Nominal + tolerans: “Son 20 ölçümün tamamı tolerans dışı”
//Barkoda göre nominal ve tolerans sözlüğü (spec) kullanır.
//Son 20 ölçümün hepsi |ölçüm - nominal| > tol ise hit.
// === Parametreler ===
const string TAG_WEIGHT  = "MAIN_Simu.fScaleWeight";
const string TAG_BARCODE = "MAIN_Simu.sBarcode";
const int WINDOW = 20;                         // son N örnek
var BARCODE_MAX_AGE = TimeSpan.FromSeconds(10);
// Barkod → (nominal, tolerans)
static readonly Dictionary<string, (double nominal, double tol)> spec
    = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BRD-ABC"] = (500.0, 10.0),
        ["BRD-XYZ"] = (350.0, 7.0),
        // ... buraya diğer barkodlar
    };
// === Kalıcı durum ===
static string? currentBarcode = null;
static DateTime currentBarcodeTs = DateTime.MinValue;
static readonly Dictionary<string, List<double>> byBarcode = new(StringComparer.OrdinalIgnoreCase);
// === Yardımcılar ===
bool IsWeight(string a) => a?.Contains(TAG_WEIGHT, StringComparison.OrdinalIgnoreCase) == true;
bool IsBarcode(string a) => a?.Contains(TAG_BARCODE, StringComparison.OrdinalIgnoreCase) == true;
void Push(string key, double v)
{
    if (!byBarcode.TryGetValue(key, out var list))
        byBarcode[key] = list = new List<double>(WINDOW);
    list.Add(v);
    if (list.Count > WINDOW) list.RemoveAt(0);
}
// === Akış ===
var addr = Event.Address ?? "";
// Barkod güncelle
if (IsBarcode(addr))
{
    var s = Event.Value?.ToString();
    if (!string.IsNullOrWhiteSpace(s))
    {
        currentBarcode = s.Trim();
        currentBarcodeTs = DateTime.UtcNow;
    }
    return false;
}
// Ağırlık ise trendi değerlendir
if (IsWeight(addr))
{
    var v = Event.ValueAsDouble();
    if (!v.HasValue) return false;
    if (string.IsNullOrWhiteSpace(currentBarcode) ||
        (DateTime.UtcNow - currentBarcodeTs) > BARCODE_MAX_AGE)
        return false;
    var bc = currentBarcode!;
    if (!spec.TryGetValue(bc, out var rule)) return false;
    Push(bc, v.Value);
    var list = byBarcode[bc];
    if (list.Count < WINDOW) return false;
    // Trend koşulu: son 20'nin tamamı tolerans dışı
    int outCnt = list.Count(x => Math.Abs(x - rule.nominal) > rule.tol);
    if (outCnt == WINDOW)
    {
        return H.Hit($"Barkod={bc}: son {WINDOW} ölçümün hepsi tolerans dışı. " +
                     $"(nominal={rule.nominal:F1}, tol=±{rule.tol:F1})");
    }
}
return false;
//Değiştir: spec sözlüğünü gerçek barkodlarına göre doldur. “Tamamı” yerine “en az 15’i” dersen aşağıdaki satırı if (outCnt >= 15) yapabilirsin.
//2) Z-score trend: “Son 20 ölçümün tamamı |z| > kSigma ve aynı yönde”
//Pencere içinden μ ve σ hesaplanır.
//Son 20 ölçümün hepsi eşik üstü (|z| > k) ve aynı yönde (hepsi pozitif sapma veya hepsi negatif) ise hit.
// === Parametreler ===
const string TAG_WEIGHT  = "MAIN_Simu.fScaleWeight";
const string TAG_BARCODE = "MAIN_Simu.sBarcode";
const int WINDOW = 20;
const int MIN_SAMPLES = 20;    // bu örnekte tam 20 istiyoruz
const double K_SIGMA = 2.5;   // trend için daha yumuşak eşik
var BARCODE_MAX_AGE = TimeSpan.FromSeconds(10);
// === Kalıcı durum ===
static string? currentBarcode = null;
static DateTime currentBarcodeTs = DateTime.MinValue;
static readonly Dictionary<string, List<double>> byBarcode = new(StringComparer.OrdinalIgnoreCase);
// === Yardımcılar ===
bool IsWeight(string a) => a?.Contains(TAG_WEIGHT, StringComparison.OrdinalIgnoreCase) == true;
bool IsBarcode(string a) => a?.Contains(TAG_BARCODE, StringComparison.OrdinalIgnoreCase) == true;
void Push(string key, double v)
{
    if (!byBarcode.TryGetValue(key, out var list))
        byBarcode[key] = list = new List<double>(WINDOW);
    list.Add(v);
    if (list.Count > WINDOW) list.RemoveAt(0);
}
double Mean(List<double> xs) => xs.Count == 0 ? 0 : xs.Average();
double Std(List<double> xs)
{
    if (xs.Count < 2) return 0;
    var m = xs.Average();
    var v = xs.Sum(x => (x - m) * (x - m)) / (xs.Count - 1);
    return Math.Sqrt(v);
}
// === Akış ===
var addr = Event.Address ?? "";
// Barkod güncelle
if (IsBarcode(addr))
{
    var s = Event.Value?.ToString();
    if (!string.IsNullOrWhiteSpace(s))
    {
        currentBarcode = s.Trim();
        currentBarcodeTs = DateTime.UtcNow;
    }
    return false;
}
// Ağırlık → trend kontrol
if (IsWeight(addr))
{
    var v = Event.ValueAsDouble();
    if (!v.HasValue) return false;
    if (string.IsNullOrWhiteSpace(currentBarcode) ||
        (DateTime.UtcNow - currentBarcodeTs) > BARCODE_MAX_AGE)
        return false;
    var bc = currentBarcode!;
    Push(bc, v.Value);
    var list = byBarcode[bc];
    if (list.Count < MIN_SAMPLES) return false;
    var mu = Mean(list);
    var sig = Std(list);
    if (sig <= 0) return false;
    // Her ölçüm için z işareti aynı mı? Hepsi > +k veya hepsi < -k?
    int posStrong = list.Count(x => (x - mu) / sig > +K_SIGMA);
    int negStrong = list.Count(x => (x - mu) / sig < -K_SIGMA);
    if (posStrong == list.Count || negStrong == list.Count)
    {
        var dir = posStrong == list.Count ? "pozitif" : "negatif";
        return H.Hit($"Barkod={bc}: son {list.Count} ölçümün tamamı {dir} yönde, |z| > {K_SIGMA}. " +
                     $"μ={mu:F2}, σ={sig:F2}");
    }
}
return false;
//Varyantlar:
//“Tamamı” yerine “en az 18’i” gibi yumuşat: if (posStrong >= 18 || negStrong >= 18).
//“Aynı yönde” şartını kaldırıp sadece |z| > k çoğunluğuna bak: if (posStrong + negStrong >= 18).
