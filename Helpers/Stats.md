
# H.Stats Dokümantasyonu (Unicode Matematik – GitHub Uyumlu)

Bu dokümantasyon, tüm matematiksel formüller **Unicode** kullanılarak yazılmıştır.  
LaTeX, HTML veya MathJax içermez — GitHub Markdown görüntüleyicisinde **%100 doğru görünür**.

---

# 1) Temel İstatistik Fonksiyonları

## Min(tag, n)
```
min(x) = en küçük değer (x₁, x₂, …, xₙ)
```

## Max(tag, n)
```
max(x) = en büyük değer (x₁, x₂, …, xₙ)
```

## Average(tag, n)
```
μ = (x₁ + x₂ + … + xₙ) / n
```

## StdDev(tag, n)
Popülasyon standart sapması:
```
σ = √( ((x₁−μ)² + (x₂−μ)² + … + (xₙ−μ)²) / n )
```

## Range(tag, n)
```
range = max(x) − min(x)
```

---

# 2) Konum Ölçüleri

## Median(tag, n)
```
Tek n:   median = x₍ₙ₊₁₎∕₂
Çift n:  median = (x₍ₙ∕₂₎ + x₍ₙ∕₂₊₁₎) / 2
```

## Percentile(tag, n, p)
Interpolasyonlu yüzdelik hesaplama:
```
rank = (p / 100) × (n − 1)
frac = rank − floor(rank)

percentile = x₍floor(rank)₎ × (1 − frac)
            + x₍ceil(rank)₎  × frac
```

## MedianAbsoluteDeviation(tag, n)
```
MAD = median( |xᵢ − median(x)| )
```

## PercentileRank(tag, n, value)
```
PercentileRank = 100 × ( count(xᵢ ≤ value) / n )
```

---

# 3) Zaman Serisi – Skaler Fonksiyonlar

## Ewma(tag, n, alpha)
```
EWMA₀ = x₀
EWMAₜ = α·xₜ + (1−α)·EWMAₜ₋₁
```

## ZScore(tag, n)
```
Z = (xₗₐₛₜ − μ) / σ
```

## Diff(tag, n)
```
Δ = xₙ − x₁
```

## Rate(tag, n, dt)
```
rate = (xₙ − x₁) / ((n − 1) × dt)
```

---

# 4) Windowed Fonksiyonlar

## LastNMovingAverage(tag, n, window)
```
MAₖ = (xₖ + xₖ₊₁ + … + xₖ₊₍w−1₎) / w
```

## LastNZScores(tag, n)
```
Zᵢ = (xᵢ − μ) / σ
```

---

# 5) Eşik Kontrolleri

## LastNExceedsThreshold
```
Any value where (xᵢ < lowerLimit) OR (xᵢ > upperLimit)
```

## LastValueExceedsThreshold
```
x_last < lowerLimit OR x_last > upperLimit
```

---

# 6) Trend ve Türevsel Fonksiyonlar

## LastNTrendSlope(tag, n)
Basit lineer regresyon eğimi:
```
slope = ( n·Σ(xᵢ·yᵢ) − (Σxᵢ)(Σyᵢ) ) 
        --------------------------------
        ( n·Σ(xᵢ²) − (Σxᵢ)² )
```

## LastNAverageRateOfChange(tag, n)
```
ROC = ( (x₂−x₁) + (x₃−x₂) + … + (xₙ−x₍ₙ₋₁₎) ) / (n − 1)
```

## LastNAverageAcceleration(tag, n)
```
ACC = Σ( xᵢ₊₂ − 2·xᵢ₊₁ + xᵢ ) / (n − 2)
```

---

# 7) Korelasyon

## Correlation(tag1, tag2, n)
Pearson korelasyonu:
```
corr =   Σ( (xᵢ−μₓ)(yᵢ−μᵧ) )
        ------------------------------------------
        √(Σ(xᵢ−μₓ)²) × √(Σ(yᵢ−μᵧ)²)
```

---

# 8) LagCorrelation

```
for lag ∈ [−maxLag … +maxLag]:
    hizalanmış x ve y serileri için corr(lag) hesapla

bestLag = argmax( |corr(lag)| )
```

---

# 9) LinearRegressionXY(tagX, tagY)

## Slope (Eğim)
```
slope = Σ( (xᵢ−μₓ)(yᵢ−μᵧ) )  /  Σ( (xᵢ−μₓ)² )
```

## Intercept (Kesişim)
```
intercept = μᵧ − slope × μₓ
```

---

# 10) Alias Fonksiyonlar
| Alias | Gerçek Fonksiyon |
|-------|------------------|
| LastNMax | Max |
| LastNMin | Min |
| LastNRange | Range |
| LastNMedian | Median |
| LastNDelta | Diff |
| LastNAverageDelta | LastNAverageRateOfChange |

---

Bu doküman GitHub için %100 optimize edilmiştir.
