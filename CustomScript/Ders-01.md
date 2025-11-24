# Ders 01
> - **Event** nedir?
> - **Event.Address** nedir?
> - **Event.Value** nedir?

⭐ **Event** Nedir?

**Event**, PLC’de bir veride değişiklik oluştuğunda veya tetiklendiğinde haber veren bir bildirim mekanizmasıdır.
Yani sistem “şu adresteki veri değişti” veya “şu olay gerçekleşti” diye haber gönderir. 

Veri değişmediği takdirde **Event** gelmez. Kodun ```Apply to Live``` yapılmasından sonra ilk defa değişiklik olan tag adresi ve değeri algılanır. Örneğin; false olan bir sensör verisi true olduğunda **Event** ile Edge Detection yaptığımızda, ilk rising ya da falling edge algılanmaz. 

Değişmeyen tag okuma veya ilk değişimde programda algılanması istediğimiz özellikler (Edge Detection gibi) farklı bir komut kullanarak ulaşılabilir yapılmıştır. H.Last komutlarında bu konu detaylı şekilde anlatılmıştır.
 
### 🔧 PLC'de Event ne zaman oluşur?

Aşağıdaki durumların herhangi biri gerçekleştiğinde:

- Bir değişkenin değeri değişince

- Bir bit set/reset edilince

- Bir input output sinyali değişince

#### 🧪 Test:
  - PLC'de değişme sıklığı 3 s altında olan bir tage abone olun.
  - Custom Scripti açın.
  - Abone olunan tagi include kutusuna kopyalayın.
  - Script Editore  ``` H.Console(Event.Address); ``` yazın.
  - Apply Live butonuna tıklayın. Seçtiğiniz tag isminin Konsol penceresinde olduğunu göreceksiniz.
  - Bu testi include penceresine birden fazla tag koyarak tekrarlayın.

> **Event** plc de abone olunan verinin değerinin değişmesi durumunda oluşur. Bu bir digital ya da analog sinyal olabilir.
> Her değer değiştiğinde bir **Event** oluşturur. Event'i oluşturan tag in adresi **Event.Address** olarak program tarafından okunur.
 
    
> Bu Event'i tetikleyen tagin son değeri ise **Event.Value** olarak okunur.
 
  ## 🔢 Event Value nedir?
  > O anda tetiklenen değişkenin yeni değeridir
  > örn: 24.58

  ### 📝 Event veri akışı örneği

  

  | **Event Address** |	**Event Value** |
  |----------------------|-------------|
  | GVL_Var.fTempActual	| 28.42 |
  | GVL_Var.fPresActual	| 48.14 |
  | GVL_Var.fTempActual	| 28.96 |
  | GVL_Var.fTempActual	| 28.23 |


Şimdi 
    
```csharp
    H.Console(Event.Address); 
    H.Console(Event.Value);
```
yazın.
Bu test ile programın çalışma mantığını tam olarak anlamış olacaksınız.

💊 Ekstra Bilgi:
> Event sıklıkları taglarde farklı olabilir. Bu yüzden event ile alınan verilerin sayıları farklı çıkması normaldir.
İleride feature vector ya da analiz için csv çıktıları alındığında  verilerin nasıl aynı timestamp ile hizalandığı konusu
detayları ile anlatılacaktır.

Peki **Event.Value** ile gelen verinin tipi nedir.

📍 C# da verinin tipini (string, int32, vb) öğrenmek için Script Editöre:
```csharp
  var x = 5;
  H.Console(x.GetType());
```
  yazın.
> 💊 *İpucu*: ```Apply to Live``` <img width="143" height="42" alt="image" src="https://github.com/user-attachments/assets/ed981cea-a556-4fa0-90a0-fe8985fdde77" /> butonuna tıklayın. Unutmayın ki programın çalışması için **Event** oluşturan bir tagin ```include``` içinde olması yeterli. ```Include``` içinde 2 tag veya daha fazla varsa hepsi **Event** oluşturur. ```Event.Address``` ve ```Event.value``` birlikte geleceği için yönetilmesi daha kolaydır.

```Apply to Live``` butonuna bastığınızda Status Bar'da <img width="268" height="24" alt="image" src="https://github.com/user-attachments/assets/108f4779-bd49-49a2-9dab-ec1f4b711916" /> yazdığını teyit edin. Compiler hata verdiyse ya da farklı bir problem varsa hatanın ne olduğu burada yazılır.


<img width="250" height="45" alt="image" src="https://github.com/user-attachments/assets/1e455925-2462-4777-bc7e-600d30349e31" />


🚨Her **Event** olduğunda program bir kere koşacaktır. ```Include``` içine yazılan tagi programda kullanmasanız bile program yalnızca **Event** oluştuğunda çalışır.

Şimdi
```csharp
  H.Console(Event.Value.GetType());
```
yazın ve çalıştırın. **Event.Value** tipinin **System.String** olduğunu görün.
Peki PLC verileri **int, double, float** formatında. Ama bize gelen veri **String**. Bu bir sorun mu?
  
🟢 Hadi gelin ikinci derste bu sorunu çözelim. 


  




