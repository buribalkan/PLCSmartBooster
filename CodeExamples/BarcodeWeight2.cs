//1) Barkoda göre kayan pencere + Z-score (genel çözüm)
//Bir “ağırlık” tag’i ve bir “barkod” tag’i var varsayıyorum.
//Barkod geldiğinde geçerli barkodu güncelliyoruz.
//Ağırlık geldiğinde geçerli barkoda yazarak o barkodun son N ağırlık penceresini tutuyoruz.
//Yeterli örnek varsa (minSamples), anlık ölçüme z-score hesaplayıp |z| > k ise hit üretiyoruz.
// ---- Parametreler ----
const string TAG_WEIGHT  = "MAIN_Simu.fScaleWeight"; // ağırlık verisi (double)
const string TAG_BARCODE = "MAIN_Simu.sBarcode";     // barkod/veri (string)
const int WINDOW = 20;   // son N ölçüm
const int MIN_SAMPLES = 8;   // en az bu kadar örnek olmadan alarm yok
const double K_SIGMA = 3.0; // |z| > k ise anomali
var BARCODE_MAX_AGE = TimeSpan.FromSeconds(10); // barkod okuması bayatlamasın
// ---- Kalıcı durum (script static alanlar) ----
static string? currentBarcode = null;
static DateTime currentBarcodeTs = DateTime.MinValue;
static Dictionary<string, List<double>> byBarcode = new(StringComparer.OrdinalIgnoreCase);
// ---- Yardımcılar ----
double Mean(List<double> xs) => xs.Count == 0 ? 0 : xs.Average();
double Std(List<double> xs)
{
    if (xs.Count < 2) return 0;
    var m = xs.Average();
    var v = xs.Sum(x => (x - m) * (x - m)) / (xs.Count - 1);
    return Math.Sqrt(v);
}
void PushSample(string key, double v)
{
    if (!byBarcode.TryGetValue(key, out var list))
        byBarcode[key] = list = new List<double>(WINDOW);
    list.Add(v);
    if (list.Count > WINDOW) list.RemoveAt(0);
}
bool IsWeight(string addr) => addr?.Contains(TAG_WEIGHT, StringComparison.OrdinalIgnoreCase) == true;
bool IsBarcode(string addr) => addr?.Contains(TAG_BARCODE, StringComparison.OrdinalIgnoreCase) == true;
// ---- Olay işleme ----
var addr = Event.Address ?? "";
// Barkod olayı ise güncelle
if (IsBarcode(addr))
{
    var s = Event.Value?.ToString();
    if (!string.IsNullOrWhiteSpace(s))
    {
        currentBarcode = s.Trim();
        currentBarcodeTs = DateTime.UtcNow;
        // sadece bilgi amaçlı (hit üretmiyoruz)
        // H.Reason($"Barkod={currentBarcode}");
    }
    return false;
}
// Ağırlık olayı ise pencereye yaz ve test et
if (IsWeight(addr))
{
    var v = Event.ValueAsDouble();
    if (!v.HasValue) return false;
    // Barkod geçerli mi?
    if (string.IsNullOrWhiteSpace(currentBarcode) ||
        (DateTime.UtcNow - currentBarcodeTs) > BARCODE_MAX_AGE)
    {
        // barkod güncel değil -> sadece pencere dışı
        return false;
    }
    var bc = currentBarcode!;
    PushSample(bc, v.Value);
    var list = byBarcode[bc];
    if (list.Count < MIN_SAMPLES) return false;
    // z-score
    var mu = Mean(list);
    var sig = Std(list);
    if (sig <= 0) return false;
    var z = (v.Value - mu) / sig;
    if (Math.Abs(z) > K_SIGMA)
    {
        return H.Hit(
            $"Barkod={bc}, ağırlık={H.Format(v)}; μ={mu:F2}, σ={sig:F2}, |z|={Math.Abs(z):F2} > {K_SIGMA}");
    }
}
return false;
//İpucu: Gerçek tag isimlerin farklı ise TAG_WEIGHT ve TAG_BARCODE’u düzenle. Barkod ve ağırlık farklı anlarda geliyorsa BARCODE_MAX_AGE ile eşlemenin tazeliğini kontrol ediyoruz.
//2) Barkoda göre “nominal ağırlık” + tolerans kontrol (hedefli çözüm)
//Bazı markaların/barkodların nominal ağırlıkları biliniyorsa, her barkod için hedef + tolerans tanımlayıp doğrudan sapmaya göre alarm üretebilirsin.
// ---- Parametreler ----
const string TAG_WEIGHT  = "MAIN_Simu.fScaleWeight";
const string TAG_BARCODE = "MAIN_Simu.sBarcode";
var BARCODE_MAX_AGE = TimeSpan.FromSeconds(10);
// Barkod -> (nominal, tolerans) sözlüğü
static Dictionary<string, (double nominal, double tol)> spec = new(StringComparer.OrdinalIgnoreCase)
{
    // örnekler
    ["BRD-ABC"] = (nominal: 500.0, tol: 10.0),  // 500 ±10
    ["BRD-XYZ"] = (nominal: 350.0, tol: 7.0),
    // ... diğerleri
};
// ---- Kalıcı durum ----
static string? currentBarcode = null;
static DateTime currentBarcodeTs = DateTime.MinValue;
bool IsWeight(string addr) => addr?.Contains(TAG_WEIGHT, StringComparison.OrdinalIgnoreCase) == true;
bool IsBarcode(string addr) => addr?.Contains(TAG_BARCODE, StringComparison.OrdinalIgnoreCase) == true;
// ---- Olay işleme ----
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
// Ağırlık gelince değerlendir
if (IsWeight(addr))
{
    var v = Event.ValueAsDouble();
    if (!v.HasValue) return false;
    if (string.IsNullOrWhiteSpace(currentBarcode) ||
        (DateTime.UtcNow - currentBarcodeTs) > BARCODE_MAX_AGE)
        return false;
    var bc = currentBarcode!;
    if (!spec.TryGetValue(bc, out var rule)) return false; // bu barkod için kural yok
    var diff = Math.Abs(v.Value - rule.nominal);
    if (diff > rule.tol)
    {
        return H.Hit(
            $"Barkod={bc}, {H.Format(v)} ≠ {rule.nominal:F1} (sapma={diff:F1} > tol={rule.tol:F1})");
    }
}
return false;
//Notlar / İyileştirmeler
//Z-score yaklaşımını robust hale getirip median + MAD (Median Absolute Deviation) kullanabilirsin; darbe / sıçrama verileri daha az etkiler.
//Ağırlık ve barkod aynı telegram içinde gelmiyorsa ama küçük gecikmeli geliyorsa BARCODE_MAX_AGE ile eşlemede sorun yaşamazsın.
//Spam’i önlemek için rule’da CooldownMs ayarla (örneğin 1000–3000ms).
//Ekstra olarak, “son 20 ölçümün tamamı tanım dışına kaydı” gibi “trend” şartları da yazılabilir (örn. list.All(x => |x-μ|>tol)).
