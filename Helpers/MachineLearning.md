# MlHelper API Documentation

Automatically generated summary of functions, descriptions, signatures, and examples.

## Feature Helpers

### `float[]? FeatureVectorFromLastN(string tag, int n, bool normalize = false)`
Builds 1D feature vector from last N values with optional normalization.

**Example**
```csharp
var fv = H.ML.FeatureVectorFromLastN("Temp", 50, normalize: true);
```

## Linear Regression

### `(double slope, double intercept)? LinearRegression(string tag, int n)`
Performs simple linear regression over last N samples.

**Example**
```csharp
var lr = H.ML.LinearRegression("Pressure", 100);
```

## K-Means (1D)

### `int[]? KMeans1D(string tag, int n, int k, int iterations = 20, int? seed = null)`
1D clustering over last N numeric values.

**Example**
```csharp
var labels = H.ML.KMeans1D("Vibration", 200, 3);
```

## Anomaly Detection

### `bool AnomalyZScore(string tag, int n, double threshold = 3.0)`
Z‑score anomaly detection.

### `bool AnomalyIqr(string tag, int n, double kappa = 1.5)`
IQR‑based anomaly detection.

### `double? EwmaChangeScore(string tag, int n, double alpha = 0.3)`
EWMA distance score.

## ONNX Inference

### `float[]? OnnxPredict(float[] features, string modelPath, string inputName, string outputName)`
Generic ONNX inference.

### `double? OnnxScoreFromLastN(...)`
Extracts features then runs ONNX.

### `bool OnnxIsolationForestAnomaly(...)`
Isolation Forest–style thresholding.

### `bool OnnxOneClassSvmAnomaly(...)`
One‑Class SVM anomaly decision.

## ML.NET PCA

### `bool TrainRandomizedPcaFromTag(string modelKey, string tag, ...)`
Train PCA anomaly model.

### `double? ScoreRandomizedPca(string modelKey, string tag, int window)`
Score latest window.

## Time‑Series Spike/ChangePoint

### `bool IidSpike(string tag, int n, ...)`
Spike detection.

### `bool IidChangePoint(string tag, int n, ...)`
Change‑point detection.

## 64D Feature Extractor

### `double[]? FeatureVector64FromLastN(string tag, int n, double dt)`
64‑dimensional engineered feature vector.

### `bool LogFeatureVector64FromLastN(...)`
Log 64D feature to CSV.

### `float[]? OnnxPredictFromFeature64LastN(...)`
Use 64D feature as ONNX input.
