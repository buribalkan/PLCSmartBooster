# PLCSmartBooster

> Endüstriyel proseslerde gerçek zamanlı veri analizi, trend modelleme, feature extraction, anomali / instability indeksleme, erken uyarı, ve akıllı öneri üretimi sağlayan; tamamen read-only, güvenli ve script-tabanlı bir analiz motoru.

> PLCSmartBooster; sensör trendlerini, proses geri bildirimlerini ve ekipman davranışını anlamak için gelişmiş istatistiksel ve sinyal işleme metodlarını kullanır. PLC’ye hiçbir şekilde yazma yapmaz, yalnızca okur, analiz eder ve kullanıcıya öneri üretir.

Özellikler

1) FeatureVector128 – Gelişmiş 128 Boyutlu Analiz Çekirdeği

Her veri penceresi için 100+ özellik çıkarır:

- **Ortalama, std, RMS, min–max, range**
- **Slope, curvature, jerk, jerkRms (2. ve 3. türev davranışları)**
- **Segment istatistikleri (4 bölge mean/std/slope/rms)**
- **Spektral özellikler (centroid, spread, flatness, crest factor)**
- **Band enerjileri (8-band FFT decomposition)**
- **Harmonicity, domFreq, domMag**
- **AC/DC energy ratio**
- **Spike metrics**
- **Instability Index**




Bu yapı, hem mekanik (titreşim), hem proses (sıcaklık PV, seviye, debi), hem de kalite kontrol sinyallerine uygulanabilir.








