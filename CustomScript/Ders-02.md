Script Editöre,

```csharp
var a = Event.Value;
H.Console(a);
H.Console(a.GetType());
```
yazın. Apply Live butonuna basın. Konsoldaki değerleri kontrol edin.
Şimdi Event.Value değerini double yani ondalıklı sayı tipine dönüştüreceğiz. Abone olunan tag in ondalıklı bir değer olduğunu varsayıyorum

Dikkat:
b sayıya dönüşmezse exception fırlatır.
✅ 1) double.Parse
String’i double olarak parse eder.
```csharp
double a = double.Parse(Event.Value.ToString());
H.Console(a.GetType());
```

Neden System.String için .ToString() methodu kullandık. Çünkü compiler Event.Value değerini object olarak değerlendireceği için 
.ToString() methodu ile compilerın hata vermemesi sağlanır.

double.Parse yöntem olarak doğru olsa bile bizim kullanacağımız parse kodu daha güvenli bir kod olmalıdır. Örneğin:

✅ 2) double.Parse (Culture ayarlı)

Eğer değer 25,34 gibi virgüllü geliyorsa:
```csharp
double a = double.Parse(Event.Value, new System.Globalization.CultureInfo("tr-TR"));
```
Eğer değer 25.34 gibi noktalı geliyorsa:
```csharp
double a = double.Parse(Event.Value, System.Globalization.CultureInfo.InvariantCulture);
```
✅ 3) double.TryParse (en güvenli yöntem)

String dönüşmeyebilir diyorsan TryParse kullan:
```csharp
if (double.TryParse(Event.Value, out double a))
{
    // a şimdi double
}
```

Virgül ihtimali varsa:
```csharp
if (double.TryParse(
        Event.Value, 
        System.Globalization.NumberStyles.Any,
        new System.Globalization.CultureInfo("tr-TR"),
        out double a))
{
    // a double olarak kullanılır
}
```
Tabiki kullanıcı H.Console(Event.Value); ile gelen veri tipini göreceği için hangi kodu kullanacağına kara vermesi zor olmayacaktır.

Eğer çok sağlam olmasını istiyorsanız 

⭐ Önerilen güvenli ve kültür bağımsız kombinasyon

Hem 25,34 hem 25.34 formatını destekler:
```csharp
double a;

if (!double.TryParse(b, System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture, out a))
{
    double.TryParse(b, System.Globalization.NumberStyles.Any,
        new System.Globalization.CultureInfo("tr-TR"), out a);
}
```
bu kod tam istediğiniz sonuçları hatasız verecektir.

Ondalık sayıların . veya , ile ayrılması çok önemlidir. Çünkü csv çıktıları almaya başladığımızda sütunları , ile ayıracağımız için ondalıklı sayıların . ile ayrılmış olması bizim için daha uygun olacaktır.

✅ 1) String içinde değiştirme (en basit yöntem)

Sadece metin dönüştürmek istiyorsan:

✔ Virgül → Nokta
```csharp
string s = "12,34";
s = s.Replace(',', '.');   // "12.34"
```
✔ Nokta → Virgül
```csharp
string s = "12.34";
s = s.Replace('.', ',');   // "12,34"
```
✅ 2) Parse ederken otomatik dönüştürme (önerilen yöntem)

Sayısal işleme girecek değerler için CultureInfo kullan.

✔ Noktalı değer varsa (12.34) → doğru parse etmek:
```csharp
double d = double.Parse("12.34",
    System.Globalization.CultureInfo.InvariantCulture);
```
✔ Virgüllü değer varsa (12,34) → doğru parse etmek:
```csharp
double d = double.Parse("12,34",
    new System.Globalization.CultureInfo("tr-TR"));
```
✅ 3) Sayıyı string’e çevirirken nokta veya virgül seçmek
✔ Daima nokta istiyorsan:
```csharp
double d = 12.34;
string s = d.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
// "12.34"
```
✔ Daima virgül istiyorsan:
```csharp
string s = d.ToString("F2", new System.Globalization.CultureInfo("tr-TR"));
// "12,34"
```
✅ 4) Her ihtimali yakalamak (virgül veya nokta olabilir)

PLC’den bazen noktalı, bazen virgüllü veri gelebilir. Bu durumda:
```csharp
string raw = Event.Value.ToString();
double result;

if (!double.TryParse(raw, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out result))
{
    double.TryParse(raw, System.Globalization.NumberStyles.Any,
        new System.Globalization.CultureInfo("tr-TR"), out result);
}

```
Bu kod:

12.34 → parse eder

12,34 → parse eder

Hata vermez

🎯 En çok kullanılan pratik dönüşüm örneği
✔ Virgüllü → Noktalı
```csharp
string normalized = raw.Replace(',', '.');
```
✔ Noktalı → Virgüllü
```csharp
string normalized = raw.Replace('.', ',');
```
Dönüşümler için bu kadar kod yeterli. 
Mademki gelen verilere double dnüşümü yaptık hadi gelin bu sayıların hakkını verelim


