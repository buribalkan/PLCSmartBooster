# SpectralDoctor (FFT tabanlı analiz)

### SpectralDoctor (FFT tabanlı analiz) her türlü sinyale uygulanabilir mi?

> Kısa yanıt: Evet, prensip olarak her türlü analog (sürekli değerli) sinyale uygulanabilir, 
ancak sonuçların anlamlı olması için bazı koşullar gerekir.
Bu koşullar sağlanmadığında FFT yine çalışır, fakat çıkan frekans bilgisi doğru olmayabilir veya yanlış yorumlanabilir.

> ### 1) Uygulanabilir sinyal tipleri

SpectralDoctor şu sinyal tiplerinde doğrudan kullanılabilir:

#### 1.1 Analog ölçüm verileri

- Basınç
- Sıcaklık
- Debi
- Seviye
- Hız
- Tork
- Akım/gerilim
- Vibrasyon (IMU, accelerometer)
Bu sinyaller FFT için uygundur.

#### 1.2 Dijital gibi olan fakat analog ölçüm içeren sinyaller

ON/OFF davranışı olsa bile, noise, switching frekansı, chatter analiz edilebilir.

#### 1.3 Gürültülü sensörler

FFT zaten “peak + noise” ayrımı yaptığı için oldukça verimlidir.

> ### 2) FFT’nin doğru çalışabilmesi için gereken koşullar

#### 2.1 Sabit örnekleme periyodu

SpectralDoctor içinde:

- sampleIntervalSec = 0.1 s

Bu değişmezse sorun yok.

Örnekleme süresi değişiyorsa FFT yanlış frekans verir.

#### 2.2 Yeterli sinyal dinamiği (varyans)

Eğer sinyal flatline ise:
- TotalEnergy düşer
- “noActivity = true” olur
- FFT çalışır ama anlamlı peak çıkmaz
Bu zaten script içinde handle ediliyor.

#### 2.3 Sinyal yeterince uzun olmalı

FFT penceren (“N örnek”) sinyalin karakteristiğini temsil etmelidir.

> Örneğin:
> 0.1 s örnekleme
> 512 nokta → 51.2 s zaman penceresi

Eğer olayın frekansı 0.001 Hz (=1000 s periyot) ise görünmez.

> ### 3) FFT ile analiz için uygun olmayan sinyaller

#### 3.1 Prosesin çok yavaş değiştiği sinyaller

> Örn:
> Tank seviyesi (saatlik değişiyor)
> Fırın sıcaklığı (çok yavaş drift)

Bu durumda FFT spektrumu düşük frekansta toplanır ve anlamlı peak çıkmaz.
Yine çalışır ama tahmin gücü zayıf olur.

#### 3.2 Açıkça “event-driven” olan sinyaller

> Örneğin:
> 30 dakikada bir gelen darbe
> Batch proses trigger’ları
**FFT** bu olayları “geniş bant noise” gibi algılar.
Bu sinyaller için **time-domain** analiz daha uygundur.

#### 3.3 Yoğun saturasyon veya clipping

Sensör 0–100% aralığında sürekli saturasyona giriyorsa:
- Frekans içeriği bozulur
- FFT sahte harmonikler üretir
Script yine çalışır fakat hatalı yorum çıkar.

> ### 4) Bu script hangi proses problemlerini algılayabilir?
Aşağıdaki problemler FFT ile doğrudan tespit edilir:

#### 4.1 Limit-cycle oscillation

PID yanlış ayarlandığında oluşan düzenli salınım.

- Peak prominence → Güçlü

- Peak frequency → Stabil

SpectralDoctor bunu **OscillationIndex** ile yakalıyor.

#### 4.2 Çalkantı (process chatter)

Regülasyon kararsız fakat düzenli değil.

**HighFreqFrac** yükselir.

#### 4.3 Sensör bozulması

- Boşluk yapma

- Titreşim

- Noise artması

- Oil-hammer

- Kötü grounding

- Analog kart gürültüsü

**HighFreqFrac** → yükselir

**TotalEnergy** → artar

**Peak** genişler

#### 4.4 Sensör donması / paralel sabit değer

**TotalEnergy** → çok düşür

**NoActivity**=true olur.

#### 4.5 Mekanik osilasyonlar

- Fan imbalance
- Pompa pulsation
- Motor belt-slip
- Bearing fault (alçak frekans bölgeleri)
**FFT** ile doğrudan görülebilir.

#### 4.6 Kayış-Kasnak kayması

Bu özel olarak:
- Fundamental belt frequency
- Slip fark frekansı
- Harmonikler

şeklinde görünür.
SpectralDoctor’ın peak prominence + multiple peaks ile tespiti çok iyidir.

> ### 5) Özet: Her sinyale uygulanabilir mi?

|Sinyal tipi| FFT uygun mu?| Sebep|
|--------------|---------|-------|
|Hızlı/orta dinamikli analog sinyal  |Evet  |Titreşim/kontrol analizine uygun|
|Gürültülü sensör   |Evet  |Noise dağılımı ölçülebilir|
|Yavaş drift sinyali |Kısmen  |Peak bulamaz|
|Event-driven sinyaller |Hayır |FFT anlamlı değil|
|Flatline Çalışır |ama anlamsız  | Enerji sıfıra yakın|
|Dijital (0/1)  |Kısmen   |Switching frekansı çıkar|

### Genel sonuç:

**SpectralDoctor** çoğu endüstriyel analog sinyal için uygundur ve proses sağlığını doğru şekilde analiz eder. 
Önemli olan sinyallerin niteliklerini bilip analiz yöntemini ona uygunşekilde yapmaktır.

