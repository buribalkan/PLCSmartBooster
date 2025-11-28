# acEnergyRatio 
PLC sinyallerinde özellikle PID tuning, mekanik stabilite, titreşim analizi ve proses kontrol kararlılığı açısından çok kritik bir metriktir. Aşağıda bunu daha derinlemesine açıklıyorum.

### acEnergyRatio nedir?

acEnergyRatio = AC (salınım/titreşim) bileşen enerjisi / toplam sinyal enerjisi

Total = DC + AC

AC = sinyalin değişken kısmı

DC = sinyalin ortalaması
Dolayısıyla:
- AC ↑ → sinyal daha çok osile ediyor
- DC ↑ → sinyal sabit bir ofsete oturmuş

AC’nin baskın olması, sinyalin stabil olmadığını, sürekli değiştiğini, bir yerde “çalışma noktası” bulamadığını gösterir.

### PLC Perspektifinde AC Enerjisi Ne Demek?

PLC ve endüstriyel sensör sinyallerinde AC bileşeni, genellikle sistemin:

- titreştiğini,
- osile olduğunu,
- gürültü üretmeye başladığını,
- mekanik boşlukların veya rezonansın devreye girdiğini,
- PID kontrolün dengesiz çalıştığını
gösterir. AC ne kadar fazlaysa, sistem o kadar “hareketli ve kararsızdır”.

### PID Tuning’de AC Enerjisi Neden Önemli?

PID kontrol döngüsünde:

- P fazla → osilasyon başlar
- I fazla → overshoot artar
- D yetersiz → titreşim bastırılamaz
Gecikme/ölçüm gürültüsü → sinyal zıplar

Bu bozuklukların hepsi ölçüm sinyalinin AC bileşenini artırır.

Bu nedenle:
> acEnergyRatio PID kararsızlığının direkt ölçüsüdür.

### Yüksek acEnergyRatio neyi gösterir?

Aşağıdaki durumların hepsi AC enerjisini yükseltir:

**1. PID osilasyonu (under-damped)**

- Sinyal sürekli ± yönde salınır.

- Çok yüksek AC → düşük stabilite.

**2. Faz gecikmesi / sensör gecikmesi**

- PID gecikmeye tepki veremez → salınım artar.

**3. Rezonans frekansı yakalanması**

- Fan, motor, pompa, servo çalışırken rezonans bandına girilmesi.

**4. Mekanik gevşeme / rulman bozukluğu**

- Titreşim → AC bileşenleri yükseltir.

**5. Yük değişimi**

- Sinyal sabit kalamaz, enerji sürekli dalgalanır.

### Düşük acEnergyRatio neyi gösterir?

- Sistem stabil
- Sinyal ideal “DC + minimum ripple” şeklinde
- PID iyi ayarlanmış
- Mekanik sorun yok
- Gürültü düşük
- Sinyal çalışma noktasını bulmuş

Yani ideal üretim hattı davranışı.

**PLC Yorumu**

> acEnergyRatio
> 
|Aralık|Miktar|Yorum|
|---|-----|----|
|0.0–0.2 |Çok düşük |Sistem taş gibi stabil|
|0.2–0.4 |Normal |Hafif gürültü, doğal vibrasyon|
|0.4–0.6 |Orta |PID tuning gözden geçirilmeli|
|0.6–0.8 |Yüksek |Osilasyon var, mekanik problem olabilir|
|0.8–1.0 |Çok yüksek |Ağır titreşim, rezonans, PID patlıyor|


# PID Tuning Rehberi (acEnergyRatio’ya Göre)

**Sinyal stabilitesini değerlendiren temel özellik:**

- acEnergyRatio = AC Enerjisi / Toplam Enerji

- Yani sinyaldeki salınım / dalgalanma oranı.

**1) acEnergyRatio < 0.20  → Sistem çok stabil**

**Gözlem**

- Sinyal neredeyse DC
- Titreşim minimum
- PID kontrol döngüsü iyi ayarlanmış
- Düzgün mekanik yapı

**Önerilen PID Ayarı:**

Tuning gerekli değil
- P – mevcut seviyede kalabilir
- I – uzun integral (yavaş birikim) uygundur
- D – gerek yok ya da düşük tutulabilir

**Uyarılar**

- Aşırı stabilite gereksiz düşük tepkisellik anlamına gelebilir (örneğin setpoint değişiminde yavaş yanıt)
- Sinyal “over-damped” olabilir

**2) 0.20 ≤ acEnergyRatio < 0.40 → Hafif dinamik bölge (normal)**

**Gözlem**

- Doğal süreç gürültüsü veya hafif dinamik hareket
- PID genel olarak stabil fakat sinyal hayattadır

**Önerilen PID Ayarı**

- P – bir miktar artırılabilir (cevap hızını artırmak için)
- I – orta seviye
- D – gerekmeyebilir ama hafif artırılabilir

**Uyarılar**

- Hız & stabilite dengesi iyi
- Sürece bağlı olarak “ideal bölge” olabilir

**3) 0.40 ≤ acEnergyRatio < 0.60 → Azalan stabilite, osilasyon başlangıcı**

**Gözlem**

- PID döngüsü under-damped olabilir
- Sinyal dalgalanıyor ama kaçmıyor
- Geri besleme agresif çalışıyor

**Önerilen PID Ayarı**

- P – az azalt (salınımları azaltmak için)
- I – integral süresini uzat (integral agresifliği düşer)
- D – artır (damping için çok etkili)

**Hedef**

Over-tuning yerine yumuşak bir dengeleme

**Uyarılar**

- Proses ineranslıysa (yüksek Gc, yüksek gecikme): I çok agresif olabilir
- Sensör gürültüsü varsa D dikkatle eklenmeli

**4) 0.60 ≤ acEnergyRatio < 0.80 → Belirgin osilasyon, kararsızlık eşiği**

**Gözlem**

- Sinyal gözle görülebilir şekilde zıplıyor
- PID döngüsü aşırı agresif
- Mekanik titreşim ihtimali yüksek

**Önerilen PID Ayarı**

- P – anlamlı şekilde azalt
- I – belirgin şekilde azalt / integral anti-windup zorunlu
- D – artır (D damping sağlar, osilasyonu keser)

**ekstra kontroller:**

- Sensör gecikmesi veya filtre eksikliği olabilir
- Aktüatör gecikmesi varsa (valf, servo) P + I mutlaka düşürülmeli
- Gerekirse input’a low-pass filtre eklenebilir

**5) acEnergyRatio ≥ 0.80 → PID patlamaya çok yakın / rezonans var**

**Gözlem**

- Aşırı salınım
- Sistem “çınlıyor”
- Mekanik rezonans bandına girilmiş olabilir
- PID kararsız

**Önerilen PID Ayarı (kritik durum)**

- P – büyük oranda azalt
- I – minimuma çek
- D – dikkatli şekilde artır. Feedforward kullanılıyorsa → geri çek

**Mutlaka kontrol edilmesi gerekenler**

- Sensör gecikmesi (ölçüm smoothing gerekli olabilir)
- Mekanik gevşeklik, boşluk, rulman sorunu
- Band-pass rezonans kontrolü
- Aktüatör çıkışında saturasyon (PID wind-up yapıyor olabilir)

**Ek mühendislik tavsiyesi**

acEnergyRatio bu seviyede ise:
PID tuning yapmadan önce mekanik/elektriksel problemi dışlamak zorunludur.

**PID Parametre Optimizasyonu İçin Genel Rehber (acEnergyRatio Bağımlı)**

**PID Tuning Rehberi – acEnergyRatio’ya Göre**

| acEnergyRatio Aralığı | Gözlem | Önerilen PID Ayarı | Uyarılar / Notlar |
|----------------------|--------|---------------------|--------------------|
| **< 0.20** (Çok stabil) | - Sinyal neredeyse DC<br>- Titreşim minimum<br>- PID iyi ayarlanmış<br>- Mekanik yapı düzgün | - Tuning gerekli değil<br>- P: mevcut seviyede<br>- I: uzun (yavaş birikim)<br>- D: yok veya düşük | - Aşırı stabilite düşük tepkisellik yaratabilir<br>- Setpoint değişimine yavaş yanıt<br>- “Over-damped” olabilir |
| **0.20 – 0.40** (Hafif dinamik / normal) | - Hafif dinamik hareket<br>- Doğal gürültü<br>- Genel stabil | - P: biraz artırılabilir<br>- I: orta<br>- D: düşük–orta | - Hız & stabilite dengesi iyi olabilir |
| **0.40 – 0.60** (Azalan stabilite, osilasyon başlangıcı) | - Under-damped yapı<br>- Dalgalanma artmış<br>- Geri besleme agresif | - P: biraz azalt<br>- I: süreyi uzat (daha yumuşak)<br>- D: artır | - Proses ataletliyse I çok agresif olabilir<br>- Sensör gürültüsünde D dikkatli eklenmeli |
| **0.60 – 0.80** (Belirgin osilasyon, kararsızlık eşiği) | - Sinyal zıplıyor<br>- PID agresif çalışıyor<br>- Mekanik titreşim ihtimali | - P: ciddi azalt<br>- I: ciddi azalt (anti-windup şart)<br>- D: artır | - Sensör gecikmesi olabilir<br>- Aktüatör gecikmesinde P+I düşürülmeli<br>- Gerekirse low-pass filtre |
| **> 0.80** (Rezonans / PID patlama eşiği) | - Aşırı salınım<br>- Sistem çınlıyor<br>- Mekanik rezonans olası<br>- PID kararsız | - P: büyük ölçüde azalt<br>- I: minimum<br>- D: kontrollü artır<br>- Feedforward varsa azalt | - Sensör gecikmesi & smoothing<br>- Mekanik gevşeklik / boşluk kontrolü<br>- Rezonans bandı analizi<br>- Aktüatör saturasyonu (wind-up riski) |

**Genel PID Optimizasyon Tablosu**


| acEnergyRatio | P Kazancı | I Süresi | D Kazancı | Açıklama |
|--------------|-----------|----------|-----------|----------|
| **< 0.20** | Aynen | Uzun | Düşük | Stabil |
| **0.20 – 0.40** | Biraz ↑ | Orta | Düşük–Orta | Normal |
| **0.40 – 0.60** | Biraz ↓ | Uzun ↑ | Orta–Yüksek ↑ | Başlangıç osilasyon |
| **0.60 – 0.80** | Çok ↓ | Çok ↓ | Yüksek ↑ | Belirgin osilasyon |
| **> 0.80** | Aşırı ↓ | Minimum | D kontrollü ↑ | Kararsızlık / rezonans |






