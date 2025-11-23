# 🧩 Script Helpers (`H.`)

`ScriptHelpers` provides a compact set of helper functions accessible through the `H.` prefix inside scripts.  
They simplify value conversion, text matching, timing, and diagnostic output.


---
# 1. History / Son Değer (Last) | (Son Değerler - LastN) Fonksiyonları

`🎯 LastN, zaman serisi veriler üzerinde çalışan tüm algoritmaların temelini oluşturan veri penceresini (data window / sliding window) sağlar.`

`İstatistiksel analizlerde — örneğin ortalama, standart sapma, trend tespiti, EMA gibi indikatörler — tek bir değer yeterli değildir. Sistem, geçmişteki belirli sayıda örneğe ihtiyaç duyar.`

**`H.LastN(tag, n)`** tam olarak bu pencereyi sağlar. Programın üzerinde çalıştığı tüm istatistiksel ve analitik süreçlerin temel taşıdır. Çünkü sistemin karar alabilmesi için yalnızca tek bir ölçüme değil, belirli bir zaman aralığındaki verilerin tamamına ihtiyacı vardır.

## 🔹 Retrieve Last Sample Values

| Function | Description | Example |
|-----------|--------------|----------|
| `H.Last(tag)` | Returns the last recorded string value for a given tag. | `var s = H.Last("Temperature");` |
| `H.LastDouble(tag)` | Returns the last recorded numeric value as `double?`. | `if (H.LastDouble("Voltage") > 5.0)` |
| `H.LastBool(tag)` | Returns the last recorded boolean value as `bool?`. | `if (H.LastBool("IsActive") == true)` |


> ### 📘 Makine Öğrenimi Veri Hattı İçin Temel Altyapı
> Gerçek zamanlı PLC verisinin **sliding window** yapısıyla işlenmesi,
> güçlü, kararlı ve yüksek doğrulukla çalışan bir  
> **Makine Öğrenimi veri hattı (pipeline)** kurulması için sağlam bir zemin hazırlar.
> Bütün bunların, veri toplanırken yapılabilmesi, çok güçlü bir sistem altyapısı sağlar.

```nginx
Son 20 örnek → 20’lik pencere
Son 50 örnek → 50’lik pencere
Son 200 örnek → 200’lük pencere
```

```csharp
public string? Last(string tag)
public string[]? LastN(string tag, int n)
public double? LastDouble(string tag)
public bool? LastBool(string tag)
public double? LastNAverage(string tag, int n)
public double? LastNStdDev(string tag, int n)
```
### 📌 Last(tag)
```csharp
/// <summary>
/// Verilen tag'in son değerini string olarak döndürür.
/// Bağlı değilse null döner.
/// </summary>
public string? Last(string tag)
```
#### ✅ Örnek Kullanım

```csharp
var sonDeger = H.Last("GVL_Var.fTempActual");
```

### 📌 LastN(tag, n)
```csharp
/// <summary>
/// Verilen tag'in son N değerini string[] olarak döndürür.
/// Null değerler "null" olarak normalize edilir.
/// </summary>
public string[]? LastN(string tag, int n)
```
#### ✅ Örnek Kullanım

```csharp
/// GVL_Var.fTempActual tagine ait son alınacak 30 değer. İstatistiksel hesaplamalar için ideal.
string tag = "GVL_Var.fTempActual";
int n = 30;
var xsRaw = H.LastN(tag, n);
```
> lastN kullanarak son 10 değerin ortalamasını bulmak. 10 değere ulaşıldığında sürekli olarak son 10 değerin ortalamasını almaya devem eder.
```csharp
using System.Globalization;
int n = 10;
var raw = H.LastN("MAIN_Simu.fBoxX[1]", n);
H.Console(string.Join(", ",
    raw.Select(x =>
    {
        var d = H.Double(x);
        return d.HasValue 
            ? d.Value.ToString("F2", CultureInfo.InvariantCulture) 
            : "null";
    })
));
// 10 değer biriktiğinden emin olmak için.
// Aksi takdirde ilk değerler geldiğinde ortalama almaya başlar.
// 10 lu pencere mantığına uygun olmaz.
if (raw == null || raw.Length < n)
{
    H.Print($"Need {n} samples, have {(raw==null ? 0 : raw.Length)}");
    return false;
}
var arr = H.LastN("MAIN_Simu.fBoxX[1]", 10)
           .Select(H.Double)
           .Where(v => v.HasValue)
           .Select(v => v.Value)
           .ToArray();

double avg = arr.Average();
H.Console($"{avg:F2}");
```
> ### 🖥️ Konsol Çıktısı
> ```
> 06:22:38.575  Info  100,40
> 06:22:38.573  Info  95.60, 96.80, 98.00, 98.80, 100.00, 100.80, 102.00, 102.80, 104.00, 105.20
> 06:22:33.545  Info  -64,24
> 06:22:33.544  Info  -69.20, -68.00, -67.20, -66.00, -64.80, -63.60, -62.80, -61.60, -60.00, -59.20
> 06:22:25.846  Info  -123,68
> 06:22:25.845  Info  -128.80, -127.60, -126.40, -125.60, -124.40, -123.20, -122.00, -120.40, -119.60, -118.80
> 06:22:19.101  Info  -145,72
> 06:22:19.090  Info  -150.80, -149.60, -148.80, -147.60, -146.40, -145.20, -144.00, -142.80, -141.60, -140.40
> 06:22:14.087  Info  Need 10 samples, have 1
> 06:22:14.087  Info  -180.00
> ```


### 📌 LastDouble(tag)
```csharp
/// <summary>
/// Son değeri double? olarak döndürür. Çevrilemiyorsa null.
/// </summary>
public double? LastDouble(string tag)
```
#### ✅ Örnek Kullanım
```csharp
var setP= H.LastDouble(spTag);
```

### 📌 LastBool(tag)

```csharp
/// <summary>
/// Son değeri bool? olarak döndürür.
/// </summary>
public bool? LastBool(string tag)
```
#### ✅ Örnek Kullanım
```csharp
var isTrue = H.LastBool("GVL_Var.fTempActual");
```

### 📌 LastNAverage(tag, n)

```csharp
/// <summary>
/// Son N sayısal değerin ortalamasını döndürür.
/// </summary>
public double? LastNAverage(string tag, int n)
```
#### ✅ Örnek Kullanım
```csharp
var avg = H.LastNAverage("GVL_Var.fTempActual", 32);
```
### 📌 LastNStdDev(tag, n)
```csharp
/// <summary>
/// Son N değerin standart sapmasını hesaplar.
/// </summary>
public double? LastNStdDev(string tag, int n)
```
#### ✅ Örnek Kullanım
```csharp
var std = H.LastNStdDev("GVL_Var.fTempActual", 20);
```
`🎯 Son N değerin standart sapması, yardımcı fonksiyon kullanmadan hesaplanmak istenirse`
```csharp
var xs = H.LastN("GVL_Var.fTempActual", 20);
if (xs == null || xs.Length == 0)
    return null;
// string[] → double?[]
var values = xs
    .Select(x => H.Double(x))
    .Where(v => v.HasValue)
    .Select(v => v.Value)
    .ToArray();
if (values.Length == 0)
    return null;
// Ortalama
double mean = values.Average();
// Toplam kare farkı
double sumSq = values.Sum(v => (v - mean) * (v - mean));
// Standart sapma
double std = Math.Sqrt(sumSq / values.Length);
```

> **var std = H.LastNStdDev("GVL_Var.fTempActual", 20); ve diğer istatistiksel yardımcılar kod tekrarını önler ve temiz bir yapı sağlar. Ama istenirse c# kodları ve kütüphaneleri kullanarak da custom scriptte çalışan kodlar yazmak mümkündür. Bu şekilde farklı fonksiyonlar da tanımlanabilir.** 










---

# 2. Log işlemleri için Durum ve Sebep Fonksiyonları

> Reason, Hit ve Fail fonksiyonları; script içinde belirlenen kuralların, koşulların veya tetikleyicilerin neden gerçekleştiğini kullanıcıya bildirmek amacıyla tasarlanmış temel log fonksiyonlarıdır. Tanımlanan kuralların (sıcaklık 30 dereceyi geçiyor mu? Geçerse bildir) gerçekleşme durumunda kullanıcıya log oluşturarak bilgi verilmesi sağlanır. Üç fonksiyonda benzer şekilde kullanılır. H.Hit, Rule Hit (Tanımlanan durum gerçekleşti) mantığına uygun olduğu için kullanılması önerilen fonksiyondur.
## 🔹 Reason & Control Flow

| Function | Description | Example |
|-----------|--------------|----------|
| `H.Reason("text")` | Stores a human-readable reason string (usually for later access). | `H.Reason("Temperature too high")` |
| `H.Hit("text")` | Sets a reason and returns `true`. Shortcut for `return H.Hit("...")`. | `return H.Hit("Valid temperature range")` |
| `H.Fail("text")` | Sets a reason and returns `false`. | `return H.Fail("Sensor not found")` |


```csharp
public void Reason(string text)
public bool Hit(string text)
public bool Fail(string? text = null)
```
### 📌 Reason(text)

```csharp
/// <summary>
/// Script'in çalıştığı adım için açıklayıcı sebep metni ayarlar.
/// Trade/condition açıklamak için kullanılır.
/// </summary>
/// <param name="text">Sebep açıklaması</param>
public void Reason(string text)
```
#### ✅ Örnek Kullanım
```csharp
H.Reason("Motor akımı artmasına rağmen Tork değişmiyor.");
```
### 📌 Hit(text) → bool
```csharp
/// <summary>
/// Sebebi ayarlar ve true döndürür. Genelde 'return H.Hit("...")' şeklinde kullanılır.
/// </summary>
/// <param name="text">Sebep açıklaması</param>
/// <returns>true</returns>
public bool Hit(string text)
```
#### ✅ Örnek Kullanım
```csharp
if (process.Value < 30)
    return H.Hit($"Process value: {process.Value}, 30 altında");
```
### 📌 Fail(text) → bool
```csharp
/// <summary>
/// Sebebi ayarlar ve false döndürür.
/// </summary>
/// <param name="text">Sebep veya null</param>
/// <returns>false</returns>
public bool Fail(string? text = null)
```
#### ✅ Örnek Kullanım
```csharp
if (volume < 1000)
    return H.Fail("Hacim yetersiz");
```

> Aşağıda 3 sensör verisine abone (subscribe) olunduğu ve yalnızca MAIN_Simu.bPlasticAtWork (**H.Like(Event.Address, "MAIN_Simu.b*Sensor")**) sensöründen gelen verilerin dikkate alındığı bir durum verilmiştir. Buradaki kural tanımı nedeni ile, sadece tag adresinin ve tag değerinin log edilmesine neden olur. **Event.Address**, **Event.Value** her veri değişiminden sonra gelen değerlerdir. Bir kaç değer aboneliği varsa, filtrelenerek istenen tagin değeri üzerinde işlem yapılır.
<img width="978" height="591" alt="image" src="https://github.com/user-attachments/assets/0db7627a-22df-46ca-bb9a-7ff3e9fb330c" />














---





# 3. Konsol / Log Fonksiyonları

## 🔹 Console Logging

| Function | Description | Example |
|-----------|--------------|----------|
| `H.Console(msg)` | Writes an info-level message to the script console. | `H.Console("Starting check...")` |
| `H.ConsoleWarn(msg)` | Writes a warning message. | `H.ConsoleWarn("Low battery")` |
| `H.ConsoleError(msg)` | Writes an error message. | `H.ConsoleError("Device offline")` |
```csharp
public void Console(object? msg)
public void ConsoleWarn(object? msg)
public void ConsoleError(object? msg)

public void Print(object msg)
public void Log(object msg)
public void Write(object msg)

public void ConsoleLastN(string tag, int n)
public void ConsoleLastNLines(string tag, int n)

public void PrintOneLineById(string id, string message)
```

---

# 4. Logging / Publisher

```csharp
public void LogSample(string directory, string filename, params (string key, object? val)[] cols)
public void LogCSV(string directory, string filename, params (string key, object? val)[] cols)

public void InitPublisherService(string pubEndpoint = "tcp://*:5556",
                                 string repEndpoint = "tcp://*:5557")

public void LogPython(string stream, params (string key, double val)[] cols)
```

---

# 5. Değer Dönüşümleri

### String dönüştürücü
```csharp
public string? Str(object? s)
```

---

### Bool dönüşümleri
```csharp
public bool? Bool(string? s)
public bool? Bool(object? v)
```

---

### Double dönüşümleri
```csharp
public double? Double(string? s)
public double? Double(object? v)
```

---

### Int dönüşümleri
```csharp
public int? Int(string? s)
public int? Int(object? v)
```

---

### Long dönüşümü
```csharp
public long? Long(object? s)
```

---

### Decimal dönüşümü
```csharp
public decimal? Decimal(object? s)
```

---

### TimeSpan dönüşümü
```csharp
public TimeSpan? TimeSpan(object? s)

| Format | Meaning | Example |
|--------|----------|----------|
| `"1h 30m"` | 1 hour 30 minutes | `H.TimeSpan("1h 30m")` |
| `"1500ms"` | milliseconds | `H.TimeSpan("1500ms")` |
| `"PT2H10M"` | ISO format | `H.TimeSpan("PT2H10M")` |
| `"00:05:00"` | hh:mm:ss | `H.TimeSpan("00:05:00")` |
| `2500` | milliseconds | `H.TimeSpan(2500)` |

```

---
# 6. Sayısal Yardımcılar

## 🔹 Math Helpers

| Function | Description | Example |
|-----------|--------------|----------|
| `H.Clamp(v, min, max)` | Restrains a value within a range. | `H.Clamp(15, 0, 10)` → `10` |
| `H.Diff(a, b)` | Difference `a - b` (returns `null` if missing). | `H.Diff(10, 3)` → `7` |
| `H.IsBetween(v, min, max)` | Checks if a value is within the range. | `H.IsBetween(5, 0, 10)` → `true` |
| `H.Abs(v)` | Absolute value. | `H.Abs(-3)` → `3` |

## Clamp
```csharp
public double? Clamp(double? v, double min, double max)
```

## Diff
```csharp
public double? Diff(double? a, double? b)
```

## IsBetween
```csharp
public bool IsBetween(double? v, double min, double max)
```

## Abs
```csharp
public double? Abs(double? v)
```

---

# 7. String Yardımcılar

## Like
```csharp
public bool Like(string? text, string pattern)
```

## Contains
```csharp
public bool Contains(string? text, string frag)
```

---

# 8. Zaman Fonksiyonları

## Now
```csharp
public DateTime Now()
```

## SecondsSince
```csharp
public double SecondsSince(DateTime ts)
```

## Ago
```csharp
public string Ago(DateTime ts)
```

---

# 9. Bitmask / Flag Fonksiyonları

## IsBitSet
```csharp
public bool IsBitSet(int value, int bitIndex)
```

## ActiveCount
```csharp
public int ActiveCount(params bool[] flags)
```

## AllTrue
```csharp
public bool AllTrue(params bool[] flags)
```

## AnyTrue
```csharp
public bool AnyTrue(params bool[] flags)
```

# 9. Kısayol / Alias Fonksiyonları

```csharp
public void LN(string tag, int n)
public string? Read(string tag)
public string? Get(string tag)
public string? LastValue(string tag)
public string? Value(string tag)
public object[] ReadN(string tag, int n)
public object[] GetN(string tag, int n)
```
