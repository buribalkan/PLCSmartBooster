
# 🧩 ScriptHelpers (`H.ML`)

---


# Genel Bilgi
**MlHelper**, script kurallarında kullanılmak üzere çeşitli makine öğrenmesi işlemlerini kolaylaştırır:

- Hafif veri önişleme
- Basit model‑siz analizler (regresyon, z‑score, IQR, EWMA)
- ONNX model çalıştırma
- ML.NET içinde PCA tabanlı anomaly modelleri eğitme
- 64 boyutlu gelişmiş özellik çıkarıcı

Script içinde kullanımı:
```csharp
var fv = H.ML.FeatureVectorFromLastN("Temp", 50);
```

---

# Feature Helpers

## `float[]? FeatureVectorFromLastN(string tag, int n, bool normalize = false)`
Son **N örnekten** tek boyutlu float[] vektörü çıkarır.

### Açıklama
- Tag’e ait son N değer alınır
- Sayısal olmayanlar filtrelenir
- Normalize = true → min‑max [0..1]

### Parametreler
| Parametre | Açıklama |
|----------|----------|
| tag | Kaynak analog tag |
| n | Kaç adet son örnek |
| normalize | Min‑max normalizasyon |

### Dönüş
`float[]` veya `null`

### Örnek
```csharp
var fv = H.ML.FeatureVectorFromLastN("Temp", 120, normalize: true);
```

---

# Doğrusal Regresyon

## `(double slope, double intercept)? LinearRegression(string tag, int n)`
Son N değere göre **y = ax + b** doğrusu döndürür.

### Açıklama
x ekseni = 0..N‑1  
y ekseni = son N değer

### Dönüş
- slope = eğim
- intercept = başlangıç kesişimi

### Örnek
```csharp
var trend = H.ML.LinearRegression("Pressure", 40);
if (trend != null)
    Log(trend.Value.slope);
```

---

# KMeans (1D)

## `int[]? KMeans1D(string tag, int n, int k, int iterations = 20, int? seed = null)`
Son N değeri kullanarak tek boyutlu k‑means kümeleme yapar.

### Açıklama
- 1D veriye k‑means uygular
- Her örneğin cluster ID’sini döner
- Basit random init + 20 iterasyon

### Örnek
```csharp
var labels = H.ML.KMeans1D("Vibration", 300, k: 3);
```

---

# Anomali Algoritmaları

## 1) Z‑Score
### `bool AnomalyZScore(string tag, int n, double threshold = 3.0)`
|z| ≥ threshold ise anomaly = true

### Örnek
```csharp
if (H.ML.AnomalyZScore("Flow", 50))
    Alarm("ZScore anomaly");
```

---

## 2) IQR
### `bool AnomalyIqr(string tag, int n, double kappa = 1.5)`
Son değer, **[Q1 − κ·IQR , Q3 + κ·IQR]** dışındaysa anomaly = true

### Örnek
```csharp
if (H.ML.AnomalyIqr("Speed", 60, 2.0))
    Alarm("IQR anomaly");
```

---

## 3) EWMA Değişim Skoru
### `double? EwmaChangeScore(string tag, int n, double alpha = 0.3)`
Son değerin EWMA’dan farkının mutlak değerini döner.

### Örnek
```csharp
var score = H.ML.EwmaChangeScore("Temp", 100);
if (score > 0.5) Alarm("Sudden change");
```

---

# ONNX Tahmin

## `float[]? OnnxPredict(float[] features, string modelPath, string inputName, string outputName)`

### Açıklama
- ONNX modeli önbelleğe alınır
- features → `[1, N]` tensörüne dönüştürülür
- Tek veya çok çıkışlı modeller desteklenir

### Örnek
```csharp
var y = H.ML.OnnxPredict(fv, "model.onnx", "Input", "Output");
```

---

# ONNX Anomali Helpers

## `double? OnnxScoreFromLastN(...)`
Son N veriden feature çıkarır → ONNX modele verir → ilk skoru döndürür.

## `bool OnnxIsolationForestAnomaly(...)`
IsolationForest modelleri için skor > threshold olabilir.

## `bool OnnxOneClassSvmAnomaly(...)`
One‑Class SVM skorları genelde threshold’un altında anomali kabul edilir.

---

# ML.NET PCA

## `bool TrainRandomizedPcaFromTag(string modelKey, string tag, int historyN, int window, ...)`
### Açıklama
- historyN kadar geçmiş veri alınır
- Pencere boyutu window
- Her pencere PCA eğitim satırı olur
- Model ML.NET RandomizedPCA ile eğitilir ve cache’e kaydedilir

### Örnek
```csharp
H.ML.TrainRandomizedPcaFromTag(
    "motor1", "Vibration", historyN: 2000, window: 32);
```

---

## `double? ScoreRandomizedPca(string modelKey, string tag, int window)`
Son pencerenin anomaly skorunu döner.

### Örnek
```csharp
var score = H.ML.ScoreRandomizedPca("motor1", "Vibration", 32);
```

---

# Spike & ChangePoint (ML.NET TimeSeries)

## `bool IidSpike(string tag, int n, int confidence = 95, int pvalueHistoryLength = 24)`
ML.NET IID Spike modeli kullanır.

## `bool IidChangePoint(string tag, int n, int confidence = 95, int changeHistoryLength = 24)`
Ani değişim (change‑point) yakalar.

---

# Feature64 (Gelişmiş Özellik Çıkarıcı)

## `double[]? FeatureVector64FromLastN(string tag, int n, double dt)`
Son N örnekten **64 boyutlu mühendislik özellikleri** çıkarır.

### İlk 20 özellik:
| Index | Özellik |
|-------|---------|
|0| mean |
|1| std |
|2| min |
|3| max |
|4| range |
|5| median |
|6| p10 |
|7| p25 |
|8| p75 |
|9| p90 |
|10| iqr |
|11| EMA α=0.1 |
|12| EMA α=0.3 |
|13| slope |
|14| RMS |
|15| zeroCrossings |
|16| spikeCount |
|17| posFrac |
|18| negFrac |
|19| diff(last-first) |
|20| outlierFrac(Z>3) |

### Kullanım
```csharp
var fv64 = H.ML.FeatureVector64FromLastN("Vibration", 128, 0.02);
```

---

## CSV Loglama
### `bool LogFeatureVector64FromLastN(...)`
64D feature + metadata’yı CSV’ye ekler.

---

# Feature64 → ONNX

## `float[]? OnnxPredictFromFeature64LastN(...)`
64D feature çıkarır → ONNX modeline girer → sonuç döner.

---

