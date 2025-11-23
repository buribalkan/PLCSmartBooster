using System.Data;
//1) Sapma + Eşik birlikte
//A ve B ikisi de eşik üstünde ve aralarındaki fark > 20 ise hit.
// Tags
const string TAG_A = "MAIN_Simu.fBoxX[1]";
const string TAG_B = "MAIN_Simu.fBoxX[2]";
// Eşikler
const double TH_A = 200.0;
const double TH_B = 190.0;
const double MIN_DIFF = 20.0;
// Kalıcı cache
static double? lastA = null;
static double? lastB = null;
var addr = Event.Address ?? "";
var v = Event.ValueAsDouble();
if (!v.HasValue) return false;
// güncelle
if (addr.Contains(TAG_A)) lastA = v.Value;
if (addr.Contains(TAG_B)) lastB = v.Value;
// hesap
if (lastA.HasValue && lastB.HasValue)
{
    if (lastA.Value > TH_A && lastB.Value > TH_B)
    {
        var diff = Math.Abs(lastA.Value - lastB.Value);
        if (diff > MIN_DIFF)
            return H.Hit($"A={H.Format(lastA)} (> {TH_A}), B={H.Format(lastB)} (> {TH_B}), |A-B|={H.Format(diff)} > {MIN_DIFF}");
    }
}
return false;
//Not: Bu script’i eklerken rule’da Include’a bu iki tag’i yazman iyi olur:
//MAIN_Simu.fBoxX[1]
//MAIN_Simu.fBoxX[2]
//2) Zaman penceresi(sıralı koşul)
//A 200’ü geçtikten sonra 2 saniye içinde B 190’ı geçerse hit (veya tersine).
// Tags
const string TAG_A = "MAIN_Simu.fBoxX[1]";
const string TAG_B = "MAIN_Simu.fBoxX[2]";
// Eşikler ve pencere
const double TH_A = 200.0;
const double TH_B = 190.0;
var WINDOW = TimeSpan.FromSeconds(2);
// Kalıcı durum
static DateTime? aCrossTs = null;
static DateTime? bCrossTs = null;
var addr = Event.Address ?? "";
var v = Event.ValueAsDouble();
if (!v.HasValue) return false;
// Eşik üstüne çıkışları damgala
if (addr.Contains(TAG_A) && v.Value > TH_A) aCrossTs = DateTime.UtcNow;
if (addr.Contains(TAG_B) && v.Value > TH_B) bCrossTs = DateTime.UtcNow;
// A -> B sırası: A geçti, 2sn içinde B geçti mi?
if (aCrossTs.HasValue && bCrossTs.HasValue)
{
    if (bCrossTs.Value >= aCrossTs.Value && (bCrossTs.Value - aCrossTs.Value) <= WINDOW)
    {
        return H.Hit($"A>{TH_A} sonra {WINDOW.TotalSeconds}s içinde B>{TH_B} (Δt={(bCrossTs.Value - aCrossTs.Value).TotalMilliseconds:F0}ms)");
    }
}
// İstersen B -> A yönünü de kontrol et:
if (aCrossTs.HasValue && bCrossTs.HasValue)
{
    if (aCrossTs.Value >= bCrossTs.Value && (aCrossTs.Value - bCrossTs.Value) <= WINDOW)
    {
        return H.Hit($"B>{TH_B} sonra {WINDOW.TotalSeconds}s içinde A>{TH_A} (Δt={(aCrossTs.Value - bCrossTs.Value).TotalMilliseconds:F0}ms)");
    }
}
return false;
//İpucu: Pencereyi daraltıp/ genişletmek için WINDOW’u değiştir; rule’daki CooldownMs ile spam’i engelle.
