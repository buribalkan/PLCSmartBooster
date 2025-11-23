
# H.Stats Dokümantasyonu  
(Tam Açıklamalı + GitHub LaTeX Uyumlu)

Bu dokümantasyon, `H.Stats` altındaki tüm fonksiyonları **detaylı açıklama**, **kullanım amacı** ve **LaTeX formatlı matematiksel formüller** ile açıklar.  
Formüller GitHub’ın KaTeX desteği ile **doğrudan görüntülenebilir**.

---

# 📌 1. Temel İstatistik Fonksiyonları

---

## 🔹 **H.Stats.Min(tag, n)**  
**Açıklama:**  
Son *n* değerden **minimum** olanı döndürür.

$$
\min(x) = \min(x_1, x_2, \ldots, x_n)
$$

---

## 🔹 **H.Stats.Max(tag, n)**  
**Açıklama:**  
Son *n* değerden **maksimum** olanı döndürür.

$$
\max(x) = \max(x_1, x_2, \ldots, x_n)
$$

---

## 🔹 **H.Stats.Average(tag, n)**  
**Açıklama:**  
Son *n* değerin aritmetik ortalamasını hesaplar.

$$
\bar{x} = \frac{1}{n} \sum_{i=1}^{n} x_i
$$

---

## 🔹 **H.Stats.StdDev(tag, n)**  
**Açıklama:**  
Popülasyon standart sapması (variance / n).

$$
\sigma = \sqrt{ \frac{1}{n} \sum_{i=1}^{n} (x_i - \bar{x})^2 }
$$

---

## 🔹 **H.Stats.Range(tag, n)**  
**Açıklama:**  
Son *n* değerin aralığı (maksimum – minimum).

$$
Range = \max(x) - \min(x)
$$

---

# 📌 2. Konum (Location) Ölçüleri

---

## 🔹 **H.Stats.Median(tag, n)**  
**Açıklama:**  
Orta değer — veri sıralandığında ortadaki değer.

Tek n:
$$
Median = x_{\frac{n+1}{2}}
$$

Çift n:
$$
Median = \frac{x_{\frac{n}{2}} + x_{\frac{n}{2}+1}}{2}
$$

---

## 🔹 **H.Stats.Percentile(tag, n, p)**  
**Açıklama:**  
0–100 arasındaki p yüzdelik dilimi.

Hesap:
$$
rank = \frac{p}{100}(n-1)
$$

Interpolasyon:
$$
Percentile(p) = x_{\lfloor rank \rfloor}(1 - frac) + x_{\lceil rank \rceil}(frac)
$$

---

## 🔹 **H.Stats.MedianAbsoluteDeviation(tag, n)**  
**Açıklama:**  
Sağlam sapma ölçüsü (outlier’lara dayanıklı).

$$
MAD = Median(|x_i - Median(x)|)
$$

---

## 🔹 **H.Stats.PercentileRank(tag, n, value)**  
**Açıklama:**  
Bir değerin son n içindeki yüzdelik derecesi.

$$
PR = 100 \cdot \frac{\#(x_i \le value)}{n}
$$

---

# 📌 3. Zaman Serisi – Skaler Dönüşümler

---

## 🔹 **H.Stats.Ewma(tag, n, alpha)**  
**Açıklama:**  
Üssel ağırlıklı hareketli ortalama.

$$
EWMA_0 = x_0
$$

$$
EWMA_t = \alpha x_t + (1 - \alpha) EWMA_{t-1}
$$

---

## 🔹 **H.Stats.ZScore(tag, n)**  
**Açıklama:**  
Son değerin standart skoru (anomaliler için ideal).

$$
Z = \frac{x_{last} - \bar{x}}{\sigma}
$$

---

## 🔹 **H.Stats.Diff(tag, n)**  
**Açıklama:**  
İlk ve son değer farkı.

$$
\Delta = x_n - x_1
$$

---

## 🔹 **H.Stats.Rate(tag, n, dt)**  
**Açıklama:**  
Yaklaşık türev (değişim hızı).

$$
Rate = \frac{x_n - x_1}{(n-1)\cdot dt}
$$

---

# 📌 4. Zaman Serisi – Pencereli Dönüşümler

---

## 🔹 **H.Stats.LastNMovingAverage(tag, n, window)**  
**Açıklama:**  
Kaydırmalı pencere hareketli ortalama.

$$
MA_k = \frac{1}{w} \sum_{i=k}^{k+w-1} x_i
$$

---

## 🔹 **H.Stats.LastNZScores(tag, n)**  
**Açıklama:**  
Tüm değerlerin z-skorlarını döndürür.

$$
Z_i = \frac{x_i - \bar{x}}{\sigma}
$$

---

# 📌 5. Eşik Kontrolleri

---

## 🔹 **H.Stats.LastNExceedsThreshold(tag, n, lower, upper)**  
**Açıklama:**  
Son *n* içinde limit aşımı var mı?

```
return (xᵢ < lower) OR (xᵢ > upper)
```

---

## 🔹 **H.Stats.LastValueExceedsThreshold(tag, lower, upper)**  
**Açıklama:**  
En son değer limit dışında mı?

```
x_last < lower OR x_last > upper
```

---

# 📌 6. Trend ve Türevsel Ölçüler

---

## 🔹 **H.Stats.LastNTrendSlope(tag, n)**  
**Açıklama:**  
Basit lineer regresyon eğimi.

$$
slope =
\frac{
n\sum(x_i y_i) - (\sum x_i)(\sum y_i)
}{
n\sum(x_i^2) - (\sum x_i)^2
}
$$

---

## 🔹 **H.Stats.LastNAverageRateOfChange(tag, n)**  
**Açıklama:**  
Ardışık farkların ortalaması.

$$
ROC = \frac{ \sum_{i=2}^{n} (x_i - x_{i-1}) }{ n-1 }
$$

---

## 🔹 **H.Stats.LastNAverageAcceleration(tag, n)**  
**Açıklama:**  
İkinci türev benzeri ivme.

$$
ACC = \frac{
\sum_{i=1}^{n-2}
(x_{i+2} - 2x_{i+1} + x_i)
}{ n-2 }
$$

---

# 📌 7. İki Değişkenli Korelasyon

---

## 🔹 **H.Stats.Correlation(tag1, tag2, n)**  
**Açıklama:**  
Pearson korelasyonu (−1 ile +1 arasında).

$$
corr =
\frac{
\sum (x_i - \bar{x})(y_i - \bar{y})
}{
\sqrt{\sum(x_i - \bar{x})^2} \cdot
\sqrt{\sum(y_i - \bar{y})^2}
}
$$

---

# 📌 8. Lag Correlation (Zaman Gecikmesi Analizi)

---

## 🔹 **H.Stats.LagCorrelation(tag1, tag2, n, maxLag)**  
**Açıklama:**  
İki sinyal arasında en iyi gecikmeyi bulur.

$$
bestLag = \arg\max_{lag \in [-L..L]} |corr(lag)|
$$

---

# 📌 9. X → Y Doğrusal Regresyon

---

## 🔹 **H.Stats.LinearRegressionXY(tagX, tagY, n)**  
Eğim (Slope):

$$
slope =
\frac{
\sum (x_i - \mu_x)(y_i - \mu_y)
}{
\sum (x_i - \mu_x)^2
}
$$

Kesişim (Intercept):

$$
intercept = \mu_y - slope \cdot \mu_x
$$

---

# 📌 10. Alias Fonksiyonlar  

| Alias | Gerçek Fonksiyon |
|-------|------------------|
| LastNMax | Max |
| LastNMin | Min |
| LastNRange | Range |
| LastNMedian | Median |
| LastNDelta | Diff |
| LastNAverageDelta | LastNAverageRateOfChange |

---

Bu dosya GitHub üzerinde **LaTeX olarak %100 render edilir.**
