# H.Stats --- İstatistik & Zaman Serisi Fonksiyonları

### GitHub KaTeX Uyumlu Tam Dokümantasyon

Bu dokümantasyon, `H.Stats` içindeki tüm fonksiyonların detaylı
açıklamalarını, matematiksel tanımlarını ve kullanım amaçlarını içerir.\
Formüller GitHub'ın KaTeX desteği ile doğrudan görüntülenebilir.

------------------------------------------------------------------------

# 📌 1. Temel İstatistik Fonksiyonları

## 🔹 H.Stats.Min(tag, n)

$$
\min(x) = \min(x_1, x_2, \ldots, x_n)
$$

## 🔹 H.Stats.Max(tag, n)

$$
\max(x) = \max(x_1, x_2, \ldots, x_n)
$$

## 🔹 H.Stats.Average(tag, n)

$$
\bar{x} = \frac{1}{n} \sum_{i=1}^{n} x_i
$$

## 🔹 H.Stats.StdDev(tag, n)

$$
\sigma = \sqrt{\frac{1}{n} \sum_{i=1}^{n} (x_i - \bar{x})^2}
$$

## 🔹 H.Stats.Range(tag, n)

$$
Range = \max(x) - \min(x)
$$

------------------------------------------------------------------------

# 📌 2. Konum (Location) Ölçüleri

## 🔹 H.Stats.Median(tag, n)

### Tek:

$$
Median = x_{\frac{n+1}{2}}
$$

### Çift:

$$
Median = \frac{x_{n/2} + x_{(n/2)+1}}{2}
$$

## 🔹 H.Stats.Percentile(tag, n, p)

$$
rank = \frac{p}{100}(n-1)
$$

Interpolasyon: $$
Percentile(p) =
x_{\lfloor rank \rfloor}(1 - frac) +
x_{\lceil rank \rceil}(frac)
$$

$$
frac = rank - \lfloor rank \rfloor
$$

## 🔹 H.Stats.MedianAbsoluteDeviation(tag, n)

$$
MAD = Median( |x_i - Median(x)| )
$$

## 🔹 H.Stats.PercentileRank(tag, n, value)

$$
PR = 100 \cdot \frac{\text{count}(x_i \le value)}{n}
$$

------------------------------------------------------------------------

# 📌 3. Zaman Serisi -- Skaler Dönüşümler

## 🔹 H.Stats.Ewma(tag, n, alpha)

$$
EWMA_0 = x_0
$$ $$
EWMA_t = \alpha x_t + (1 - \alpha) EWMA_{t-1}
$$

## 🔹 H.Stats.ZScore(tag, n)

$$
Z = \frac{x_{last} - \bar{x}}{\sigma}
$$

## 🔹 H.Stats.Diff(tag, n)

$$
\Delta = x_n - x_1
$$

## 🔹 H.Stats.Rate(tag, n, dt)

$$
Rate = \frac{x_n - x_1}{(n-1) dt}
$$

------------------------------------------------------------------------

# 📌 4. Pencereli Dönüşümler

## 🔹 H.Stats.LastNMovingAverage(tag, n, window)

$$
MA_k = \frac{1}{w} \sum_{i=k}^{k+w-1} x_i
$$

## 🔹 H.Stats.LastNZScores(tag, n)

$$
Z_i = \frac{x_i - \bar{x}}{\sigma}
$$

------------------------------------------------------------------------

# 📌 5. Eşik Kontrolleri

## 🔹 H.Stats.LastNExceedsThreshold(tag, n, lower, upper)

$$
\exists x_i : (x_i < lower) \text{ or } (x_i > upper)
$$

## 🔹 H.Stats.LastValueExceedsThreshold(tag, lower, upper)

$$
x_{last} < lower \text{ or } x_{last} > upper
$$

------------------------------------------------------------------------

# 📌 6. Trend & Türevsel Ölçüler

## 🔹 H.Stats.LastNTrendSlope(tag, n)

$$
slope =
\frac{
n \sum(x_i y_i) - (\sum x_i)(\sum y_i)
}{
n\sum(x_i^2) - (\sum x_i)^2
}
$$

## 🔹 H.Stats.LastNAverageRateOfChange(tag, n)

$$
ROC = \frac{\sum_{i=2}^{n} (x_i - x_{i-1})}{n-1}
$$

## 🔹 H.Stats.LastNAverageAcceleration(tag, n)

$$
ACC =
\frac{
\sum_{i=1}^{n-2} (x_{i+2} - 2x_{i+1} + x_i)
}{n-2}
$$

------------------------------------------------------------------------

# 📌 7. Korelasyon

## 🔹 H.Stats.Correlation(tag1, tag2, n)

$$
corr =
\frac{
\sum (x_i - \bar{x})(y_i - \bar{y})
}{
\sqrt{\sum(x_i - \bar{x})^2}
\sqrt{\sum(y_i - \bar{y})^2}
}
$$

------------------------------------------------------------------------

# 📌 8. Lag Correlation

## 🔹 H.Stats.LagCorrelation(tag1, tag2, n, maxLag)

$$
bestLag = \arg\max_{lag \in [-L,L]} |corr(lag)|
$$

------------------------------------------------------------------------

# 📌 9. Linear Regression (X → Y)

## 🔹 H.Stats.LinearRegressionXY(tagX, tagY, n)

### Eğim:

$$
slope =
\frac{
\sum (x_i - \mu_x)(y_i - \mu_y)
}{
\sum (x_i - \mu_x)^2
}
$$

### Kesişim:

$$
intercept = \mu_y - slope\mu_x
$$

------------------------------------------------------------------------

# 📌 10. Alias Fonksiyonlar

  Alias               Gerçek Fonksiyon
  ------------------- --------------------------
  LastNMax            Max
  LastNMin            Min
  LastNRange          Range
  LastNMedian         Median
  LastNDelta          Diff
  LastNAverageDelta   LastNAverageRateOfChange
