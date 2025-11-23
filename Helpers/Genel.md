
# Custom Script Helpers – API Fonksiyonları

## Genel Bilgiler / Özellikler



---
# 1. History / Son Değer (Last) | (Son Değerler - LastN) Fonksiyonları

`🎯 LastN, zaman serisi veriler üzerinde çalışan tüm algoritmaların temelini oluşturan veri penceresini (data window / sliding window) sağlar.`

`İstatistiksel analizlerde — örneğin ortalama, standart sapma, trend tespiti, EMA gibi indikatörler — tek bir değer yeterli değildir. Sistem, geçmişteki belirli sayıda örneğe ihtiyaç duyar.`

**`H.LastN(tag, n)`** tam olarak bu pencereyi sağlar. Programın üzerinde çalıştığı tüm istatistiksel ve analitik süreçlerin temel taşıdır. Çünkü sistemin karar alabilmesi için yalnızca tek bir ölçüme değil, belirli bir zaman aralığındaki verilerin tamamına ihtiyacı vardır.

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

# 2. Durum ve Sebep Fonksiyonları

> Reason, Hit ve Fail fonksiyonları; script içinde belirlenen kuralların, koşulların veya tetikleyicilerin neden gerçekleştiğini kullanıcıya bildirmek amacıyla tasarlanmış temel log fonksiyonlarıdır. Tanımlanan kuralların (sıcaklık 30 dereceyi geçiyor mu? Geçerse bildir) gerçekleşme durumunda kullanıcıya log oluşturarak bilgi verilmesi sağlanır. Üç fonksiyonda benzer şekilde kullanılır. H.Hit, Rule Hit (Tanımlanan durum gerçekleşti) mantığına uygun olduğu için kullanılması önerilen fonksiyondur.


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
```

---
# 6. Sayısal Yardımcılar

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
