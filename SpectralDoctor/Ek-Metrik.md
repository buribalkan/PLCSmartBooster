# 🧠 Ek Metrik Önerileri (Gerçekten Fayda Sağlayanlar)
### 1️⃣ Signal-to-Oscillation Ratio (SOR)

“Periyotlu sinyal enerjisi / toplam enerji” oranı.

#### ➤ Ne işe yarar?

- Osilasyonun sistem enerjisindeki ağırlığını doğrudan gösterir.

- Limit-cycle’ın ne kadar “baskın” olduğunu bir sayıyla özetler.

#### ➤ Neden önemli?

**OscIndex** peak göre dominantlığı, SOR ise bütün sinyal içindeki payı anlatır.
Birlikte bakınca mükemmel teşhis sağlar.

### 2️⃣ SlewRate (dPV/dt) Histogramı / RMS
#### ➤ Ne işe yarar?

- Sinyalin ne kadar hızlı değiştiğini gösterir.

- Noise ile gerçek hızlı dinamikleri ayırmayı kolaylaştırır.

- Kontrol valflerinde sıkışma/overshoot algısında kullanılır.

#### ➤ Ek katkı:

Valf behaviour analizi için çok iyi bir erken uyarı sinyali olur.

### 3️⃣ Low-Frequency Energy Ratio (LF Energy)
#### ➤ Ne işe yarar?

- Sinyalin 0–0.005 Hz bandındaki enerjisini ölçer.

- Yavaş drift, sensör kayması (sensor bias drift), uzun süreli kazanç değişimi gibi şeyleri gösterir.

#### ➤ Ek katkı:

TotalEnergy düşük ama LF yüksekse → ölü ama sürüklenen sinyal (ör. kötü sensör).

### 4️⃣ Valf Command vs PV Coherence (Koherans Analizi)

Sistem cevabını belirli frekanslarda ölçmek için.

#### ➤ Ne işe yarar?

- Kontrol döngüsünün hangi frekanslarda iyi çalıştığını görürsün.

- PID yetersiz / aşırı agresif frekans bölgelerini tespit eder.

#### ➤ Neden değerli?

Periyodik bir sinyal varsa:

- Valf → PV koherans yüksek = proses gerçekten salınıyor

- Düşük koherans = sensör veya gürültü kaynaklı pseudo-oscillation

### 5️⃣ Symmetry Index (peak-to-trough symmetry)

Salınımın yukarı ve aşağı yönlerinin eşit olup olmadığını ölçer.

#### ➤ Ne işe yarar?

- PID’de integral windup

- Nonlinear valf davranışı

- Rekatizasyon / ölü bant

gibi sorunları ortaya çıkarır.

#### ➤ Formül:
> Symmetry = abs(peakAmplitude - troughAmplitude) / maxAmplitude

### 6️⃣ Duty-Cycle-like Bölgesel Yoğunluk Analizi

Sinyal, çalışma aralığının hangi yüzdesinde zaman geçiriyor?

#### ➤ Ne işe yarar?

- Kontrol vanası bir uçta takılı kalmış mı?

- PV aynı aralığa mı saplanmış?

- Sensör saturasyonu var mı?

Güçlü bir diagnostik metriktir.

### 7️⃣ Cepstrum Peak Analysis

#### ➤ Ne işe yarar?

- Periyodik yapının ses işleme gibi çok net algılanmasını sağlar.

- FFT’de kaçan küçük döngüleri bile çıkarır.

- PID tuning sırasında özellikle çok faydalı.
