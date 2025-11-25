# SPEKTRAL ANALİZ YORUM REHBERİ

> PLC proses verisi için mühendislik odaklı yorumlama kılavuzu

### 1. ANALİTİK ZİHİN HARİTASI

Her analog sinyal için analiz mantığı aslında 4 büyük sorudan oluşur:

- Enerji nerede yoğunlaşmış?
- Bu enerji zaman içinde periyodik mi, rastgele mi?
- Peak frekansı ne? Bu periyod proses davranışına uygun mu?
- Spektrum dar bant mı, geniş bant mı? (noise vs. salınım)

Bütün metrikler aslında bu 4 sorunun alt cevaplarını oluşturur.

> 1. TOTAL ENERGY
    
| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Sinyalin genel varyansı / aktivitesi (DC çıkarıldıktan sonra) | **TotalEnergy Değeri** | - **Çok düşük (~0’a yakın):** Sinyal neredeyse düz; sensör donmuş olabilir, proses çok yavaş olabilir veya veri ölçeği çok küçük olabilir. <br> - **Stabil büyüyor:** Proses daha hareketli hale geliyor (ör. setpoint değişimi, yük değişimi). <br> - **Stabil düşüyor:** Proses sakinleşiyor, kontrol daha iyi kapanıyor. <br> - **Not:** Mutlak değer değil **trend** önemlidir. |


> 2. HIGH FREQUENCY FRACTION (HighFreqFrac)
   
| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Sinyalin hızlı değişimleri → noise, mekanik titreşim, sensör jitter’ı | **HighFreqFrac** | - **< %5:** Çok pürüzsüz sinyal – mekanik noise yok, sensör iyi çalışıyor. <br> - **%5–25:** Normal proses değişkeni için kabul edilebilir noise seviyesi. <br> - **> %25:** Anormal yüksek frekans bileşeni — sensör jitter, vibrasyon coupling, mekanik problem olasılığı. <br> - **Not:** Sıcaklık gibi yavaş sinyallerde tipik beklenti **< %10**’dur. |







> 3. OSCILLATION INDEX (OscIndex)

| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Tek bir dar bant frekansın toplam enerjiye oranı → limit-cycle var mı? <br><br>**Formül:** `OscIndex = peakPower / totalEnergy` | **OscIndex** | - **< %20:** Sinyal random, belirgin periyodik yapı yok. <br> - **%20–40:** Hafif periyodik yapı – normal olabilir. <br> - **%40–60:** Baskın periyot – limit-cycle eğilimi. <br> - **> %60:** Belirgin kontrol osilasyonu (PID tuning sorunu). |


> 4. PEAK FREQUENCY (peakFreq)

| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Salınımın periyodu <br><br>**Formül:** `Period = 1 / peakFreq` <br><br>**Örnek:** <br>`peakFreq = 0.0176 Hz` → `Period = 56.8 s` <br>Bu şu demektir: <br>**“Sinyal yaklaşık her ~57 saniyede düzenli bir şekilde yükselip iniyor.”** | **Period** | - **1–5 s:** Mekanik vibrasyon, çok hızlı proses. <br> - **5–20 s:** Flow / pressure kontrol döngüleri. <br> - **30–120 s:** Termal proses osilasyonları (çoğunlukla PID kaynaklı). <br> - **>120 s:** Batch davranışı, büyük gecikme veya yavaş kontrol döngüsü. |



> 5. PEAK MAGNITUDE (mag)



| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Salınım genliğinin büyüklüğü (PV biriminde) <br><br>**Örnek:** `mag = 0.174` → yaklaşık **0.17 °C** genlik | **Amplitude (mag)** | **“Sinyal, ilgili peak frekansın periyodu boyunca ±0.17°C civarında salınıyor.”** |


> 6. PEAK PROMINENCE (prom)


| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Peak’in çevresine göre ne kadar baskın olduğunu gösterir. <br><br>**Prominence ≈ mag** ise → peak izole ve çok belirgin. <br>Bu, **“Sistem gerçekten bu periyotla salınıyor”** anlamına gelir. | **Prominence / Magnitude** | - **< %20:** Peak belirsiz, rastgele. <br> - **%20–70:** Orta seviye periyodiklik. <br> - **> %70:** Çok belirgin salınım (limit-cycle adayı). <br><br>**Örnek:** <br>`prom ≈ 0.1738` <br>`mag ≈ 0.1740` <br>Bu neredeyse **%100 prominence** → **saf periyodik davranış**. |

> 7. PEAK WIDTH (width)

| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Peak’in frekans ekseninde ne kadar geniş olduğu → salınım kararlı mı, dağınık mı? | **Width (Hz)** | - **< 0.01 Hz:** Çok küçük → Kararlı, neredeyse sabit periyotlu osilasyon. <br> - **0.01–0.05 Hz:** Orta → Hafif frekans drift’i. <br> - **> 0.05 Hz:** Büyük → Rastgele, kararsız periyot, noise ağırlıklı. <br><br>**Örnek:** <br>`Width = 0.0059 Hz` → çok dar bant → **“cerrahi netlikte salınım.”** |


> 8. SAĞLIK (Health)


| **Ne Gösterir** | **Nasıl Yorumlanır** | **Yorum** |
|-----------------|----------------------|-----------|
| Heuristik bir “stability” ölçüsü. <br><br>**Health değerini etkileyen ana kriterler:** <br>- **OscIndex ↑ → Health ↓** <br>- **HighFreqFrac ↑ → Health ↓** <br>- **TotalEnergy ↓ → Health ↓** <br>- **PeakProminence ↑ → Health ↓** (genelde limit-cycle işareti) | **Health** | **Örnek:** <br>`Health = 86.7` → **Grade B** <br>Bu şu anlama gelir: <br>**“İyi, ama tamamen kusursuz değil.”** |


> 9. 📊 Hızlı Durum Özeti – FFT Tabanlı Proses Analizi

| 🏷️ Metrik | 📘 Ne Anlam Taşır? | 🔍 Hızlı Yorum |
|-----------|--------------------|----------------|
| ⚡ **TotalEnergy + minEnergy** | Enerji düşük mü? Sinyal ölü mü? | Çok düşük → 🧊 Sinyal donuk / sensör ölü <br> Yükseliyor → 📈 Proses hareketli <br> Düşüyor → 📉 Proses sakinleşiyor |
| 🔊 **HighFreqFrac** | Noise var mı? | < %5 → 😌 Temiz <br> %5–25 → 🙂 Normal <br> > %25 → 🚨 Noise / jitter / vibrasyon |
| 🔁 **OscIndex** | Periyodik yapı var mı? | < %20 → 🔄 Rastgele <br> %20–40 → 🙂 Hafif periyodiklik <br> %40–60 → ⚠️ Limit-cycle eğilimi <br> > %60 → 🚨 Belirgin osilasyon |
| ⏱️ **Period (1/peakFreq)** | Periyot nedir? | Örnek: 0.0176 Hz → 56.8 s <br> Yani: “Her ~57 saniyede bir döngü” |
| 🎯 **Prominence** | Salınım ne kadar net? | < %20 → 🌫️ Belirsiz <br> %20–70 → 🙂 Orta netlik <br> > %70 → 🎯 Çok net / saf periyodik |
| 📡 **Width** | Salınım kararlı mı? | <0.01 Hz → 🔒 Kararlı <br> 0.01–0.05 → 🟡 Hafif drift <br> >0.05 → 🌪️ Kararsız / noise |
| ❤️ **Health** | Genel sağlık ne? | 90+ → 🟢 A <br> 80–90 → 🟡 B <br> 70–80 → 🟠 C <br> <70 → 🔴 Zayıf |
