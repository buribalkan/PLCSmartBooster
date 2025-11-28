Buradaki bilgiler benim özel notlarımdır. Kimseye tavsiye niteliğinde değildir. Kimseye de hitap etmemektedir

# PLCSmartBooster

> Endüstriyel proseslerde gerçek zamanlı veri analizi, trend modelleme, feature extraction, anomali / instability indeksleme, erken uyarı, ve akıllı öneri üretimi sağlayan; tamamen read-only, güvenli ve script-tabanlı bir analiz motoru.

> PLCSmartBooster; sensör trendlerini, proses geri bildirimlerini ve ekipman davranışını anlamak için gelişmiş istatistiksel ve sinyal işleme metodlarını kullanır. PLC’ye hiçbir şekilde yazma yapmaz, yalnızca okur, analiz eder ve kullanıcıya öneri üretir.

PLCSmartBooster, veri ingest → in-memory windowing → rule/script evaluation → feature extraction → ML inference → human-visible suggestion & logging akışını izleyen, script-sandbox ile güvenli, ONNX ile production-ready model çıkarma/çalıştırma yeteneği olan, modüler ve genişletilebilir bir analiz motorudur.

# Sistem Mimarisi – Teknik Dokümantasyon

## 1. Donanım / Veri Kaynağı Katmanı
**Sorumluluk:** Gerçek zamanlı proses verisini üreten PLC, RTU, DAQ sistemleri ile sensörler ve aktüatörler.  
**Protokoller:** OPC UA, ADS  
**Kritik Noktalar:**  
- Örnekleme frekansı  
- Timestamp doğruluğu  
- Tag isimlendirme standardı (role mapping)

## 2. Bağlantı / Ingest Katmanı (Adapters)
PLC bağlantılarının yönetimi, tag subscribe/poll, veri normalizasyonu ve backpressure kontrolünden sorumludur.

### Bileşenler
- `OpcUaClient`, `AdsClient`  
- `PlcPoller`, `TagSubscriptionManager`  
- `TagRoleMapper` (asset-level → kanonik tag)

### Davranış
- Gelen ham veri `PlcEvent` / `Sample` formatına dönüştürülür.  
- Timestamp, adres ve sıra numarası (seq-id) eklenir.  
- Bağlantı kopması ve QoS için retry/reconnect döngüleri yönetilir.

## 3. Mesajlaşma / Buffering Katmanı
Event’leri kısa süreli ve güvenilir şekilde tamponlar; windowing işlemlerine destek sağlar.

### Bileşenler
- `InMemoryQueue`  
- `RingBuffer`  
- `LastNCache` (tag bazında son **N** değer)

### Özellikler
- Backfill  
- Late-arrival handling  
- Tag bazlı rate-limiting

## 4. İş Mantığı / Kural Motoru (Rule Engine)
Event’lerin tanımlı kurallara göre işlenmesinden sorumludur.

### Bileşenler
- `RuleManager`  
- `CustomScriptRule` (c# scriptler)  
- `RuleRunner`, `Dispatcher`

### Davranış
- Her event için script bağlamı (`H`) oluşturulur.  
- Script çalışır, sonuç `true` ise `RuleHit` üretilir.  
- Severity, reason text ve hit log kaydedilir.

### Güvenlik
- Script sandboxing  
- Read-only API (`ScriptHelpers`)  

## 5. Script Sandbox / Helpers API
Script’lere kontrol edilmiş, güvenli, izole bir okuma API’si sağlar.

### API Örnekleri
- `H.LastN(tag, n)`  
- `H.Double(...)`  
- `H.Console(...)`, `H.LogSample(...)`  
- `H.Stats.*`
- `H.Signal.*`
- `H.ML.*` (OnnxPredict, FeatureVector…)

### Hata Yönetimi
- Script exception → kural fail-safe  


## 6. Özellik Çıkarımı (Feature Extraction)
Last-N pencere verilerinden çok boyutlu özellik vektörleri (FV64 / FV128 / FV256 / FV640…) üretir.
Gelişmiş 128 Boyutlu Analiz Çekirdeği ile feature (özellikler) online hesaplanır.

### Bileşenler
- `FeatureExtractor`  
- `MlHelper.FeatureVector*` fonksiyonları

### Davranış
- LastN verisi alınır  
- `pad` / `requireFullWindow` stratejisi uygulanır  
- Normalizasyon, EMA opsiyonları desteklenir
- Her veri penceresi için 100+ özellik çıkarır:

> - **Ortalama, std, RMS, min–max, range**
> - **Slope, curvature, jerk, jerkRms (2. ve 3. türev davranışları)**
> - **Segment istatistikleri (4 bölge mean/std/slope/rms)**
> - **Spektral özellikler (centroid, spread, flatness, crest factor)**
> - **Band enerjileri (8-band FFT decomposition)**
> - **Harmonicity, domFreq, domMag**
> - **AC/DC energy ratio**
> - **Spike metrics**
> - **Instability Index**

## 7. ML Inference Katmanı
ONNX modellerinin yüklenmesi, çalıştırılması ve çıktıların işlenmesinden sorumludur.

### Bileşenler
- `MlHelper` (ONNX cache + predict)  
- `OnnxModelManager`  
- PCA model cache

### Davranış
1. Feature vector üret  
2. (Gerekirse) scaler uygula  
3. ONNX modele input ver  
4. Score/label üret  
5. Post-process

### Özellikler
- Model reload  
- Hot-swap  
- Model versioning  
- Input/output metadata

## 8. Decision / Suggestion Katmanı (Actionability)
Kural ve ML çıktılarından operatöre yönelik okunabilir öneriler üretir.

### Bileşenler
- `SuggestionEngine`  
- `TemplateRepo`  
- `ReasonBuilder`

### Davranış Örneği
ML: **Cavitation**, suctionP düşük →  
“**Suction valfini kontrol edin, debiyi azaltın; gerekirse NPSH değerini doğrulayın.**”

### Kayıt
- ReasonText  
- LogSample

## 9. Kayıt / Telemetri / Persistans Katmanı
Loglama, metrik toplama ve persistans işlemlerinden sorumludur.

### Bileşenler
- `ConsoleLines`  
- `LogSampleWriter (CSV)`  
- `HitsStore` (in-memory + disk)  
- `MetricsCollector` (Prometheus / custom)

### İçerik
- Feature CSV (model eğitimi)  
- Inference logları (model + score)  
- Alarm geçmişi

### Retention
- Rotasyon  
- Partition  
- Offline eğitim için export

## 10. UI / Operator Konsolu
Kural yönetimi, script yazımı, telemetri ve log izleme, model yönetimi için kullanıcı arayüzü sağlar.

### Bileşenler
- WPF UI  
- ScriptEditor 
- ConsoleList  
- Rule List  
- Model Deploy UI

### Özellikler
- Tek seferlik script testi  
- State-based yeniden çalıştırma (Run Again)  
- Console filtreleme  
- PrintOneLine tek satır güncelleme  
- CSV indirme

## 11. Güvenlik / Sandbox / Governance
Script güvenliği, audit mekanizmaları

### Tedbirler
- Read-only Script Helpers API  
- Assembly referans kısıtlamaları  
- Script timeout / exception capture  
- Model signing & version control

## 12. Veri Akışı – Per-Event Örnek İşlem Sırası
1. PLC publish → `OpcUaClient` event’i yakalar.  
2. `PlcEvent` oluşturulur.  
3. `TagSubscriptionManager` → `LastNCache` güncellenir.  
4. `RuleManager` tetiklenir; uygun kurallar filtrelenir.  
5. `ScriptHelpers.Bind(...)` ile script bağlamı (`H`) hazırlanır.  
6. `CustomScriptRule` sandbox’ta çalışır.  
   - `H.LastNFeatureVector128(...)`  
   - `H.ML.OnnxPredict(...)` çağrılabilir.  
7. Script `true` dönerse `RuleHit` oluşturulur.  
8. ReasonText ve LogSample kaydedilir; UI’ya iletilir.  
9. `MetricsCollector` sayaçları günceller.


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

### Some Screenshots:

<img width="860" height="575" alt="image" src="https://github.com/user-attachments/assets/989f9ab2-c86d-4ed2-8d47-741a13de0420" />

<img width="860" height="575" alt="image" src="https://github.com/user-attachments/assets/074e1c04-5933-4411-84dc-8ba6601e38e7" />

<img width="860" height="575" alt="image" src="https://github.com/user-attachments/assets/eb05d7e6-1dce-4378-af8b-9a38f091672d" />







