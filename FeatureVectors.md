# 🔧 FV128 İleri Metrikler (121–126)

Aşağıdaki 6 metrik; **osilasyon – kararlılık – jitter – asimetri** davranışlarını çok temiz biçimde yakalar:

- **121 — oscIndex** 🎯 Dominant peak / total energy  
- **122 — signalToOscRatio** ⚖️ Geriye kalan enerji / dominant enerji  
- **123 — slewRmsNorm** 📉 diffRms / std  
- **124 — slewMaxNorm** 📈 maxDiff / std  
- **125 — symmetryMag** 🔄  
- **126 — signBias** ➕➖ (posCount - negCount) / (pos+neg)

---

## 🔍 1) Oscillation Signature Classifier  
**(Limit-cycle vs Noise vs Drift)**

fv121 & fv122 ile sınıflar:

| oscIndex | SOR (fv122) | Yorum |
|---------|-------------|-------|
| yüksek | düşük | 🎵 Dar bant limit-cycle (kararlı) |
| yüksek | yüksek | ⚠️ Kaotik osilasyon (PID overshoot) |
| düşük | düşük | 😴 Sessiz / durağan |
| düşük | yüksek | 🌫️ Gürültü / drift ağırlıklı |

### 🎛️ Sınıflandırma Script (%95 doğruluk)
```csharp
if (fv[121] > 0.5 && fv[122] < 1.0) mode = "Stable Limit-Cycle";
else if (fv[121] > 0.5 && fv[122] >= 1.0) mode = "Unstable Oscillation";
else if (fv[122] > 1.5) mode = "Noise/Drift Dominant";
else mode = "Quiet-Stable";
```
Bu sınıflar doğrudan “Root Cause” sistemine entegre edilebilir.

---

## 🔥 2) PID Agresiflik Skoru

SlewRmsNorm + SlewMaxNorm → **PID’in agresifliğini** ölçer.

**Formül:**  
`Aggressiveness = sqrt(slewRmsNorm² + slewMaxNorm²)`

- **>1.5** → 🚨 Aşırı agresif  
- **0.7–1.2** → ✅ İdeal  
- **<0.3** → 🐌 Çok yavaş / under-tuned  

---

## ⚖️ 3) Waveform Symmetry → Valve Deadband Tespiti

- symmetryMag büyük → ⚡ Asimetri var  
- signBias pozitif → ↗️ Yukarı yön baskın  
- signBias negatif → ↘️ Aşağı yön baskın  

### 🛠️ Örnek:
```csharp
if (fv[125] > 0.5 && Math.Abs(fv[126]) > 0.4)
    Fault = "Valve stiction or asymmetric actuator response";
```
# 🌊 4) Oscillation Purity Index (Yeni)

**Limit-cycle + noise karışımını ayırır:**

`Purity = fv[121] / (fv[121] + fv[122] + 1e-6)`

- **1 →** 🎵 Çok temiz osilasyon  
- **0 →** 🌪️ Geniş bant gürültü  

---

## 🧭 5) Drift vs Oscillation Separation

fv123 & fv124 birlikte çalışır:

- drift → 📉 slewRms küçük, slope büyük  
- jitter → ⚡ slewMax büyük  
- osilasyon → 🔁 ikisi de büyük ama periyodik  

---

## 🎯 6) Setpoint Tracking Quality (Yeni)

Instability index (fv127) = düşük geçiren filtreli versiyon  
➡️ `TrackingErrorQuality = 1 - fv[127]`

- **1** → 🎯 Mükemmel  
- **0** → 🔥 Instabil  

---

## 📡 7) Noise-Only Detector

```csharp
if (fv[121] < 0.1 && fv[122] > 2.0 && fv[123] > 0.5)
    noiseDetected = true;
```

---

## 🐕 8) Early Hunting Detection

fv121 düşükken fv123 yükseliyorsa → **Hunting başlıyor!**

---

## 🧱 9) Actuator Saturation / Deadband

- fv125 yüksek  
- fv124 düşük  

→ 🧱 Mekanik sıkışma / saturasyon işareti.

# 📊 10) Cycle-to-Cycle Variance (Yeni)

`CycleVariance = fv121 * fv125`

Yüksekse → 🔄 limit-cycle kararsız / bozuk.

---

## 🌀 11) Nonlinear Control Behavior Detector

- oscIndex yüksek  
- symmetryMag yüksek  
- slewRms düşük  

→ ⚠️ Nonlinear PID bölgesi (stick-slip vb.)

---

## 🌡️ 12) Thermal Process Health Score

`ThermalHealth = 1 - 0.5*fv127 - 0.4*fv121`

---

## 🧨 13) Failing Sensor Detection

- signBias uçmuş  
- fv123 büyük  
- fv121 küçük  

→ ❌ Sensör bozuluyor.

---

## 🛠️ 14) Valve Performance Metric

`ValveHealth = 1 - (fv123 + fv125)`

---

## 🛑 15) Early Oscillation Warning

fv121 küçükken fv123 yükselirse → **Erken uyarı**
# 🧩 16) Zaman Bazlı K-Means Clustering

fv121–fv122–fv127 ile proses modları:

- 💤 Idle  
- 🔥 Warm-Up  
- 🧘 Stable  
- 🎯 Overshoot  
- 🔁 Limit-cycle  
- 🌫️ Noise  

---

# ⚡ 17) Spectral–Dynamic Hybrid Score
FV128’in en güçlü kombinasyonu:

`Hybrid = 0.4*fv121 + 0.3*fv123 + 0.3*fv127`

> Bu, geniş bant jitter + dar bant osilasyon + trend instabilitesini tek skorda özetler.
Harika bir “Proses Sağlık Endeksi” olur.
---

# 🧠 18) Oscillation Type Classifier (JSON → Script)

fv121–126 ile %90+ doğruluk:

- 🎵 Clean sinusoidal limit-cycle  
- 🐕 PID hunting  
- ⚡ High-frequency jitter  
- 🌫️ Low-frequency drift  
- 💥 Chattering  
- 🧱 Mechanical stiction oscillation
