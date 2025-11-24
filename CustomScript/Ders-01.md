# Ders 01
> - Event nedir?
> - Event.Address nedir?
> - Event.Value nedir?

⭐ Event Nedir?

Event, PLC’de bir şey oluştuğunda, değiştiğinde veya tetiklendiğinde haber veren bir bildirim mekanizmasıdır.
Yani sistem sana “şu adresteki veri değişti” veya “şu olay gerçekleşti” diye haber gönderir.

### 🔧 PLC'de Event ne zaman oluşur?

Aşağıdaki durumların herhangi biri gerçekleştiğinde:

- Bir değişkenin değeri değişince

- Bir bit set/reset edilince

- Bir input output sinyali değişince

  Test:
  - PLC'de değişme sıklığı 3 s altında olan bir tage abone olun.
  - Custom Scripti açın.
  - Abone olunan tagi include kutusuna kopyalayın.
  - Script Editore  ``` H.Console(Event.Address); ``` yazın.
  - Apply Live butonuna tıklayın. Seçtiğiniz tag isminin Konsol penceresinde olduğunu göreceksiniz.
  - Bu testi include penceresine birden fazla tag koyarak tekrarlayın.

    > Event plc de abone olunan verinin değerinin değişmesidir. Bu bir digital ya da analog sinyal olabilir.
    > Her değer değiştiğinde bir Event oluşturur. Eventi oluşturan tag in adresi Event.Address olarak program tarafından okunur.
 
    
    > Bu event i tetikleyen tagin son değeri ise Event.Value olarak okunur.
 
  ## 🔢 Event Value nedir?
  >O anda tetiklenen değişkenin yeni değeridir
  >örn: 24.58

  ### 📝 Event veri akışı örneği

  

  | Event Address |	Event Value |
  |----------------------|-------------|
  | GVL_Var.fTempActual	| 28.42 |
  | GVL_Var.fPresActual	| 48.14 |
  | GVL_Var.fTempActual	| 28.96 |
  | GVL_Var.fTempActual	| 28.23 |

    Şimdi ``` H.Console(Event.Address); ``` altına 
    ``` H.Console(Event.Value); ``` yazın.
    Bu test ile programın çalışma mantığını tam olarak anlamış olacaksınız.

  Ekstra Bilgi:
  Event sıklıkları taglarde farklı olabilir. Bu yüzden event ile alınan verilerin sayıları farklı çıkması normaldir.
  İleride feature vector ya da analiz için csv çıktıları alındığında  verilerin nasıl aynı timestamp ile hizalandığı konusu
  detayları ile anlatılacaktır.

  Peki Event.Value ile gelen verinin tipi nedir.
  C# da verinin tipini (string, int32, vb) öğrenmek için Script Editöre:
  var x = 5;
  H.Console(x.GetType());
  yazın.
  İpucu: Apply Live butonuna tıklayın. Unutmayın ki programın çalışması için event oluşturan bir tagin include içinde olması yeterli.
  O her event yarattığında program bir kere koşacaktır. Include içine yazılan tagi programda kullanmasanız bile program yalnızca
  Event oluştuğunda çalışır.
  Şimdi
  H.Console(Event.Value.GetType());
  yazın ve çalıştırın. Event.Value tipinin System.String olduğunu görün.
  Peki PLC verileri int, double, float formatında. Ama bize gelen veri String. Bu bir sorun mu?
  Hadi gelin ikinci derste bu sorunu çözelim. 


  




