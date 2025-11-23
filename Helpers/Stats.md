
# H.Stats Fonksiyonları – Kullanım Dokümantasyonu (Markdown)

## 1. Temel İstatistik Fonksiyonları
### Min(tag, n)
```csharp
double? H.Stats.Min(string tag, int n)
```
Son n değerden minimum değeri döndürür.

### Max(tag, n)
```csharp
double? H.Stats.Max(string tag, int n)
```

### Average(tag, n)
```csharp
double? H.Stats.Average(string tag, int n)
```

### StdDev(tag, n)
```csharp
double? H.Stats.StdDev(string tag, int n)
```

### Range(tag, n)
```csharp
double? H.Stats.Range(string tag, int n)
```

## 2. Konum Ölçüleri
### Median(tag, n)
```csharp
double? H.Stats.Median(string tag, int n)
```

### Percentile(tag, n, p)
```csharp
double? H.Stats.Percentile(string tag, int n, double p)
```

### MedianAbsoluteDeviation(tag, n)
```csharp
double? H.Stats.MedianAbsoluteDeviation(string tag, int n)
```

### PercentileRank(tag, n, value)
```csharp
double? H.Stats.PercentileRank(string tag, int n, double value)
```

## 3. Zaman Serisi Dönüşümleri – Skaler
### Ewma(tag, n, alpha)
```csharp
double? H.Stats.Ewma(string tag, int n, double alpha = 0.3)
```

### ZScore(tag, n)
```csharp
double? H.Stats.ZScore(string tag, int n)
```

### Diff(tag, n)
```csharp
double? H.Stats.Diff(string tag, int n)
```

### Rate(tag, n, dt)
```csharp
double? H.Stats.Rate(string tag, int n, double sampleIntervalSec)
```

### MovingAverage(tag, n)
```csharp
double? H.Stats.MovingAverage(string tag, int n)
```

## 4. Zaman Serisi Dönüşümleri – Pencereli
### LastNMovingAverage(tag, n, window)
```csharp
double?[] H.Stats.LastNMovingAverage(string tag, int n, int window)
```

### LastNZScores(tag, n)
```csharp
double?[] H.Stats.LastNZScores(string tag, int n)
```

## 5. Eşik Fonksiyonları
### LastNExceedsThreshold
```csharp
bool H.Stats.LastNExceedsThreshold(string tag, int n, double lower, double upper)
```

### LastValueExceedsThreshold
```csharp
bool H.Stats.LastValueExceedsThreshold(string tag, double lower, double upper)
```

## 6. Trend ve Türevsel Fonksiyonlar
### LastNTrendSlope
```csharp
double? H.Stats.LastNTrendSlope(string tag, int n)
```

### LastNAverageRateOfChange
```csharp
double? H.Stats.LastNAverageRateOfChange(string tag, int n)
```

### LastNAverageAcceleration
```csharp
double? H.Stats.LastNAverageAcceleration(string tag, int n)
```

## 7. Korelasyon
### Correlation
```csharp
double? H.Stats.Correlation(string tag1, string tag2, int n)
```

## 8. LagCorrelation
### LagCorrelation
```csharp
(int bestLag, double bestCorr)? H.Stats.LagCorrelation(string tag1, string tag2, int n, int maxLag = 10)
```

## 9. Lineer Regresyon
### LinearRegressionXY
```csharp
(double slope, double intercept)? H.Stats.LinearRegressionXY(string tagX, string tagY, int n)
```

## 10. Alias Fonksiyonlar
- LastNMax → Max
- LastNMin → Min
- LastNRange → Range
- LastNMedian → Median
- LastNDelta → Diff
- LastNAverageDelta → LastNAverageRateOfChange
