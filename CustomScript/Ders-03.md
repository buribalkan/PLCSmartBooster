Mademki kural penceresindeyiz, Custom Scrşpt ile kod yazmaya başladık ve 
değerleri dönüştürmeyi öğrendik, o zaman ilk kuralımızı yazalım.

Kural yazmanın amacı PLC deki verilerdeki anomalileri ortaya çıkarmak. Fakat biz ilk önce 
kuralların nasıl çalıştığına bakacağız.

Kuralları çalıştırmak için kullanacağımız komut 
H.Hit 
olacak. Bu komut 
return H.Hit(Anomalinin Tanımı)
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
