
# H.Signal — SignalHelper Fonksiyon Referansı 
**Erişim:** `H.Signal.*`

---

## 📘 Boolean Durum Fonksiyonları

### ### `bool StuckBool(string tag, int n, bool? stuckValue = null)`
Boolean sinyalinin son **n** örnekte hiç değişmediğini kontrol eder.

#### **Parametreler**
- `tag` — Sinyalin etiketi  
- `n` — Kontrol edilecek örnek sayısı  
- `stuckValue`  
  - `true` → sadece hep true ise stuck  
  - `false` → sadece hep false ise stuck  
  - `null` → her iki durum da kabul  

#### **Döndürür**
- `true` → sinyal stuck  
- `false` → değil  

#### **Kullanım**
```csharp
if (H.Signal.StuckBool("ValveOpen", 10))
    Alarm("Valve stuck!");
```

---

### ### `bool RisingEdgeBool(string tag)`
False → True kenar geçişini algılar.

#### **Kullanım**
```csharp
if (H.Signal.RisingEdgeBool("StartButton"))
    Log("Start pressed!");
```

---

### ### `bool FallingEdgeBool(string tag)`
True → False kenar geçişini algılar.

#### **Kullanım**
```csharp
if (H.Signal.FallingEdgeBool("StartButton"))
    Log("Start released!");
```

---

### ### `bool DebounceBool(string tag, int requiredStableCount, bool requireTrue = true)`
Sinyalin son k örnekte hep sabit kaldığını kontrol eder.

#### **Parametreler**
- `requiredStableCount` — İstenen minimum stabil örnek sayısı  
- `requireTrue` — Stabil durum True mu olmalı?

#### **Kullanım**
```csharp
if (H.Signal.DebounceBool("DoorClosed", 5))
    Log("Door stably closed.");
```

---

### ### `bool Chatter(string tag, int n, int maxToggles)`
Son n örnekte sinyal sürekli toggling yapıyorsa chatter tespit eder.

#### **Kullanım**
```csharp
if (H.Signal.Chatter("MotorFb", 20, 5))
    Alarm("Motor feedback is noisy!");
```

---

# 📘 Analog Olay Fonksiyonları

### ### `bool Spike(string tag, int n, double absThreshold, double pctThreshold)`
Son iki örnek arasındaki ani değişimi tespit eder.

#### **Parametreler**
- `absThreshold` — Mutlak fark eşiği  
- `pctThreshold` — Yüzdesel fark eşiği  

#### **Kullanım**
```csharp
if (H.Signal.Spike("Pressure", 3, 5.0, 0.2))
    Alarm("Pressure spike!");
```

---

### ### `bool? HysteresisAnalog(string tag, int n, double low, double high)`
Analog histerezis; düşük–yüksek bant.

#### **Sonuç Değerleri**
- `true` → yüksek bölgede  
- `false` → düşük bölgede  
- `null` → belirsiz  

#### **Kullanım**
```csharp
var state = H.Signal.HysteresisAnalog("Temp", 5, 40, 60);
```

---

### ### `bool? HysteresisBool(string tag, int k, bool requireTrue = true)`
Boolean histerezis; k örnekte hep aynıysa stabil.

#### **Kullanım**
```csharp
var s = H.Signal.HysteresisBool("LevelHigh", 4);
```

---

# 📘 Zaman Tabanlı Fonksiyonlar

### ### `bool Deadman(string tag, int n, double sampleIntervalSec, double maxSilentSec)`
Bir sinyalin çok uzun süredir değişmediğini algılar.

#### **Parametreler**
- `sampleIntervalSec` — Sinyalin örnekleme aralığı  
- `maxSilentSec` — Maksimum sessiz kalma süresi  

#### **Kullanım**
```csharp
if (H.Signal.Deadman("Flow", 100, 0.1, 10))
    Alarm("Flow sensor not updating");
```

---

### ### `string? TrendDirection(string tag, int n, double threshold = 0.0)`
Trend yönünü belirler → `"up" | "down" | "flat"`

#### **Kullanım**
```csharp
var tr = H.Signal.TrendDirection("Temp", 30, 0.5);
```

---

