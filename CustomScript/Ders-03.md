

⚠️ DİKKAT
❗ ÖNEMLİ
🚨 UYARI
🔥 KRİTİK

Ayrıca alternatif olarak:

⚡

🛑

⛔

🔔

📢

❕

❗❗

⚠️⚠️

Markdown başlıklarıyla kullanmak istersen:

## ⚠️ Dikkat

### 🚨 Kritik Uyarı

> ❗ Önemli Not


“İlaç / hap / capsule” ikonları için en yaygın kullanılan emojiler şunlardır:

💊

(hap / capsule)

Alternatifler:

🧪 (deney tüpü – laboratuvar bağlamında)

🩺 (tıbbi bağlam)

🆘 (acil durum)

🚑 (ambulans)

En doğrudan “hap” anlamına gelen emoji:

👉 💊



Mademki kural penceresindeyiz, Custom Script ile kod yazmaya başladık ve 
değerleri dönüştürmeyi öğrendik, o zaman ilk kuralımızı yazalım.

Kural yazmanın amacı PLC deki verilerdeki anomalileri ortaya çıkarmak. Fakat biz ilk önce 
kuralların nasıl çalıştığına bakacağız.

Kuralları çalıştırmak için kullanacağımız komut
```csharp
H.Hit
``` 
olacak. Bu komut
```csharp
return H.Hit("Anomalinin Tanımı")
```
şeklinde çalışır. Şimdi bir Event okuyup bunu double değerine dönüştürüp değer belirli bir değerin üzerinde ise Hit oluşturmasını sağlayalım.
H.Hit çalıştığını görmek için şu anda hangi tagi kullanıyorsak o tag değeri üzerinden işlem yapalım. Benim kullandığım tagin değeri 7 ile 9 arasında çalışıyor ve 9 un üzerinde çok az kalıyor.
O değeri geçtiğinde Hit oluşturalım. Siz kendi taginiz göre ayarlama yapabilirsiniz. Seçeceğiniz değer double olsun.
```csharp
string raw = Event.Value.ToString();
double result;
if (double.TryParse(Event.Value.ToString(),
                    out result))
{
    if( result > 9.0 )
    {
        return H.Hit($"Gelen değer {result} > 9.0");
    }
}
else
{
    H.Console("Parse işlemi başarısız");
}
```
Benim test ettiğim tag ile kural ile belirlediğimiz 9 üzeri değer gelince Rule Hit olduğu için Observe sayfasında tam 
tanımladığım şekilde Hit ler oluştu.



Hit oluştururken Cooldown 1000 ms yaptım Cooldown özellikle çok hızlı değişen eventlerde çok fazla Hit yazmasını engellemek için daha da yükseltilebilir. Cooldown ilk Hit üretildikten sonra set edilen zaman kadar bekler. Böylece Hit alanının benzer alarmlarla dolması engellenir.

Test Sonucu:

<img width="800" height="500" alt="image" src="https://github.com/user-attachments/assets/f97664e3-b8f3-487e-bcc8-da27edb42b75" />

C#’ta ekrana mesaj yazdırırken hem sabit bir metni hem de bir değişkenin o anki değerini birlikte göstermek istediğimizde string interpolasyonu kullanırız. Bu özellik sayesinde metin içinde {} kullanarak değişken yerleştirebiliriz.

🔹 Sadece metin yazdırmak
```csharp
H.Console("Sensör çok sık durum değiştirdi...");
```
🔹 Metin içinde değişken kullanmak

Bir metin ile değişkeni birlikte göstermek için metnin başına $ koyarız ve değişkeni süslü parantez içine yazarız:
```csharp
int sayı = 10;
H.Console($"Sayı değeri: {sayı}");
```

Burada:

Sayı değeri: → normal string bölümü

{sayı} → değişkenin güncel değeri

Program çalıştığında çıktı şöyle olur:

Sayı değeri: 10

🔹 Birden fazla değişken kullanmak

İstediğiniz kadar değişkeni aynı string içinde kullanabilirsiniz:
```csharp
string ad = "Ahmet";
int yas = 25;
H.Console($"Ad: {ad}, Yaş: {yas}");
```
🔹 Sadece değişkeni yazdırmak

Sadece değişkenin kendisini yazdıracaksanız string interpolasyonuna gerek yok:
```csharp
H.Console(sayı);
```

Kısacası:

Normal metin: "..."

Metin + değişken: $" ... {değişken} ... "

Sadece değişken: H.Console(değişken)

#### H Helper

> H. Helper fonksiyonlar, PLC Smart Booster içerisinde tanımlanmış, PLC çalışma yapısına uygun şekilde hazırlanmış yardımcı fonksiyonların genel adıdır. Bu fonksiyonlar, programlamayı kolaylaştırır ve birçok işlemi daha okunabilir, daha pratik hale getirir. Programa özel komutlar olup c# derleyicisi tarafından tanınmaktadır. Bu kodlarda oluşacak hatalar da derleyici tarafından kullanıcıya bildirilir. 

Sık kullanılan fonksiyonlar arasında H.Console ve H.Hit bulunur. Bunlara ek olarak, veri işleme ve kontrol için süreci oldukça basitleştiren başka yardımcı komutlar da mevcuttur.

Yazacağımız programlarda verilere erişmek ve bu veriler üzerinde işlem yapmak için bu yardımcı fonksiyonları kullanacağız. Ayrıca, geliştirme sırasında Event.Address ve Event.Value değerleri, hata ayıklama (debug) sürecinde önemli ölçüde kolaylık sağlayacaktır.

Bundan sonraki derste artık bu fonksiyonları kullanmaya başlayacağız. 
