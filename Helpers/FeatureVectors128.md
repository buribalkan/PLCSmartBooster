# FEATURE VECTORS 128
___
> ## 📌 fv[0] — mean (Ortalama) 
### Tanım: 
> Sinyalin tüm örneklerinin aritmetik ortalaması. 
### PLC Uygulamaları: 
 - ✔ **Sinyalin genel seviyesini gösterir.** 
 - ✔ **Yüksek mean → proses değeri yüksek bölgede çalışıyor.**
 - ✔ **PID çıkışında mean kayması, sistemde sürekli bir ofset olduğuna işaret edebilir.**
> ## 📌 fv[1] — std (Standart sapma) 
### Tanım: 
>Sinyalin ne kadar değişken olduğunu ölçer. 
### 💊 Proses Yorumu: 
- ✔ **Yüksek std → sinyal oynak, sistem stabil değil.**
- ✔ **Düşük std → kararlı çalışma.**
- ✔ **PID aşırı agresifse std yükselir.**
> ## 📌 fv[2] — min (Minimum değer) 
### Tanım: 
>Sinyalin aldığı en düşük değer. 
### 💊 Proses Yorumu: 
- ✔ **Alt limitlere vurup vurmadığını gösterir.** 
- ✔ **Sensör arızalarında beklenmedik çökme burada yakalanır.** 
> ## 📌 fv[3] — max (Maksimum değer) 
### Tanım: 
> Sinyalin aldığı en yüksek değer. 
### 💊 Proses Yorumu: 
- ✔ **Üst limitlere, saturasyona çıkıp çıkmadığını gösterir.** 
- ✔ **Peak değerler kontrol kararlılığı için önemli.** 
> ## 📌 fv[4] — range (max - min) 
### Tanım: 
> Sinyalin toplam yayılımı. 
### 💊 Proses Yorumu: 
- ✔ **Sinyal ne kadar geniş aralıkta oynuyor?** 
- ✔ **Çok büyük range → sistemde büyük salınımlar olabilir.** 
> ## 📌 fv[5] — median (Medyan) 
### Tanım: 
> Verilerin ortadaki değeri. 
### 💊 Proses Yorumu:
- ✔ **Gürültüden etkilenmeyen merkez noktası.** 
- ✔ **mean kayıyorsa, median–mean farkı sistemde asimetri/gürültü gösterebilir.** 
> ## 📌 fv[6] — p10 (10. yüzdelik) 
### Tanım: 
> Verilerin alt %10’luk kısmının sınırı. 
### 💊 Proses Yorumu:
- ✔ **Sinyalin alt uç davranışını ölçer.**
- ✔ **Proses genelde alt değerlerde takılıyorsa p10 belirgin şekilde düşer.** 
> ## 📌 fv[7] — p25 (25. yüzdelik / Q1) 
### Tanım: 
> Verilerin alt çeyrek sınırı. 
### 💊 Proses Yorumu:
- ✔ **Sinyalin daha düşük çalışma bölgesi hakkında stabil bilgi verir.** 
- ✔ **Gürültü azsa p25 ile median arası mesafe küçüktür.** 
> ## 📌 fv[8] — p75 (75. yüzdelik / Q3) 
### Tanım:
> Verilerin üst çeyrek sınırı. 
### 💊 Proses Yorumu:
- ✔ **Üst çalışma aralığını temsil eder.** 
- ✔ **p75 ile p25 farkı, yani IQR, değişkenliği gösterir.** 
> ## 📌 fv[9] — p90 (90. yüzdelik) 
### Tanım: 
> Verilerin üst %10’luk kısmının başlangıcı. 
### 💊 Proses Yorumu:
- ✔ **Peak’e yakın davranışlar hakkında hızlı fikir verir.** 
- ✔ **Sistem aşırı yükleniyorsa p90 belirgin şekilde yukarı çıkar.** 
> ## 📌 fv[10] — mad (Median Absolute Deviation) 
### Tanım: 
> Medyana göre mutlak sapmaların medyanı. Gürültüye en dayanıklı yayılım ölçütü. 
### 💊 Proses Yorumu: 
- ✔ **Sensör gürültüsü veya küçük titreşimler için hassastır.** 
- ✔ **PID jitter’ı varsa mad yükselir.** 
- ✔ **std’ye göre anormal durumlarda daha güvenilir.** 
> ## 📌 fv[11] — iqr (Interquartile Range, p75 - p25) 
### Tanım: 
> Sinyalin orta %50’lik bölümünün genişliği. 
### 💊 Proses Yorumu: 
- ✔ **Gürültü ve osilasyon seviyesinin iyi bir göstergesi.** 
- ✔ **iqr küçük → sinyal sıkışık ve stabil.** 
- ✔ **iqr büyük → kontrol döngüsü fazla oynak.** 
> ## 📌 fv[12] — rms (Root Mean Square) 
### Tanım: 
> Sinyalin karelerinin ortalamasının karekökü. 
### 💊 Proses Yorumu:
- ✔ **Enerji/yoğunluk ölçüsü.** 
- ✔ **Titreşim analizinde özellikle önemlidir.** 
- ✔ **rms yükseliyorsa sistem daha fazla güç veya hareket üretiyor olabilir.** 
> ## 📌 fv[13] — absMean (Mutlak ortalama) 
### Tanım: 
> Tüm değerlerin mutlaklarının ortalaması. 
### 💊 Proses Yorumu:
- ✔ **Pozitif/negatif dalgalanmaları tek tarafta toplar.** 
- ✔ **AC benzeri salınımlar için anlamlıdır.** 
- ✔ **absMean yüksekse sinyal sürekli hareket halinde demektir.** 
> ## 📌 fv[14] — absStd (Mutlak değerlerin standart sapması) 
### Tanım: 
> Mutlak değerlerin değişkenliğini ölçer. 
### 💊 Proses Yorumu:
- ✔ **Hareket magnitüdünün ne kadar değişken olduğunu gösterir.** 
- ✔ **absStd yüksek → düzensiz salınımlar.** 
- ✔ **Özellikle titreşim ve akım analizinde önemli.** 
> ## 📌 fv[15] — count (Örnek sayısı) 
### Tanım: 
> Sinyaldeki toplam örnek sayısı. 
### 💊 Proses Yorumu:
- ✔ **Hesaplamaların güvenilirliği bu değerle doğru orantılıdır.** 
- ✔ **Çok düşük count → özellikler sağlıklı olmayabilir.** 
> ## 📌 fv[16] — skew (Çarpıklık) 
### Tanım: 
> Dağılımın sağa mı sola mı kaydığını gösterir. 
### 💊 Proses Yorumu:
- ✔ **Pozitif skew → üst değerlere doğru kuyruk var, ani yükselme eğilimleri olabilir.** 
- ✔ **Negatif skew → alt değerlere doğru kayma, ani düşüşler daha baskın.** 
- ✔ **Sistem dengesiz çalışıyorsa skew büyür.** 
> ## 📌 fv[17] — kurt (Basıklık / Kurtosis) 
### Tanım: 
****Dağılımın sivriliğini ölçer; uç değerlerin yoğunluğunu gösterir. 
### 💊 Proses Yorumu:
- ✔ **Yüksek kurt → Sinyalde ani pikler, sert zıplamalar var.** 
- ✔ **Düşük kurt → Daha yayvan ve stabil dağılım.** 
- ✔ **Sensör spike’ları ve ani darbeler kurt ile yakalanır.** 
> ## 📌 fv[18] — entropy (Entropi) 
### Tanım: 
> Sinyalin düzensizliğinin bilgi teorisi tabanlı ölçüsü. 
### 💊 Proses Yorumu:
- ✔ **Yüksek entropi → Sinyal çok karışık, gürültü yüksek, düzen yok.** 
- ✔ **Düşük entropi → Tekrarlayan, daha düzenli davranış.** 
- ✔ **Motor titreşimleri, bozuk enkoder sinyalleri entropiyi artırabilir.** 
> ## 📌 fv[19] — logVar (Logaritmik varyans) 
### Tanım: 
> Varyansın logaritmasını alarak geniş aralıkları sıkıştırır. 
### 💊 Proses Yorumu:
- ✔ **Gürültülü sinyallerde değişkenliğin büyüklüğünü kontrollü gösterir.** 
- ✔ **Çok büyük varyans artışları logVar ile daha okunabilir hâle gelir.** 
> ## 📌 fv[20] — cv (Coefficient of Variation — Değişim Katsayısı) 
### Tanım: 
> std / mean oranı; göreceli değişkenlik. 
### 💊 Proses Yorumu:
- ✔ **cv yüksek → Ortalama düşük ama oynaklık yüksek → istikrarsız sistem.** 
- ✔ **cv düşük → Ortalama seviyeye göre stabil çalışma.** 
- ✔ **Proses düşük değerlerde çalışırken bile osilasyonları iyi yakalar.** 
> ## 📌 fv[21] — posCount / n (Pozitif oran) 
### Tanım: 
> Pozitif değerlerin toplam değerlere oranı. 
### 💊 Proses Yorumu:
- ✔ **Sinyal daha çok pozitif bölgede mi çalışıyor?** 
- ✔ **Akım/gerilim gibi çift yönlü sinyallerde yön baskınlığını gösterir.** 
> ## 📌 fv[22] — negCount / n (Negatif oran) 
### Tanım: 
> Negatif değerlerin toplam değerlere oranı. 
### 💊 Proses Yorumu:
- ✔ **Sinyalin ne kadar süre negatif bölgede kaldığını ölçer.** 
- ✔ **Motor geri yön davranışları, çift yönlü hareketlerde anlamlı.** 
> ## 📌 fv[23] — zeroCount / n (Sıfır oranı) 
### Tanım: 
> Sıfır (veya sıfıra çok yakın) değerlerin oranı. 
### 💊 Proses Yorumu:
- ✔ **Sinyal çok sık 0 seviyesine geri dönüyorsa sistem atıl olabilir.** 
- ✔ **ADC saturasyonu veya ölü bölge davranışları burada görünür.** 
> ## 📌 fv[24] — maxAbs (Mutlak maksimum) 
### Tanım: 
> En büyük mutlak değer. 
### 💊 Proses Yorumu:
- ✔ **Sinyalin gördüğü en yüksek genlik.** 
- ✔ **Mekanik darbe, aşırı yük, ani akım çekişi gibi durumlarda yükselir.** 
> ## 📌 fv[25] — meanPos (Pozitif değer ortalaması) 
### Tanım: 
> Yalnızca pozitif değerlerin ortalaması. 
### 💊 Proses Yorumu:
- ✔ **Pozitif yöndeki tipik sinyal seviyesini gösterir.** 
- ✔ **Motor ileri yönde çalışırken güç/akım profilini anlamak için iyidir.** 
> ## 📌 fv[26] — meanNeg (Negatif değer ortalaması) 
### Tanım: 
> Negatif değerlerin ortalaması. 
### 💊 Proses Yorumu:
- ✔ **Geri yöndeki çalışma seviyesini gösterir.** 
- ✔ **meanPos / meanNeg karşılaştırması yön simetrisi hakkında bilgi verir.** 
> ## 📌 fv[27] — ratioUpper (Üst sınıra yakın değer oranı) 
### Tanım: 
> Sinyalin üst limit veya eşik değerinin yakınında kalma oranı. 
### 💊 Proses Yorumu:
- ✔ **Sistem üst kapasiteye yakın çalışıyor olabilir.** 
- ✔ **Saturasyona yaklaşma davranışı izlenebilir.** 
> ## 📌 fv[28] — ratioLower (Alt sınıra yakın değer oranı) 
### Tanım: 
> Sinyalin alt limit/alt eşik civarında kalma oranı. 
### 💊 Proses Yorumu:
- ✔ **Alt limitlere dayanma durumu varsa burada görünür.** 
- ✔ **Ölü bölge, valf kapalı konum, düşük akım vb.** 
> ## 📌 fv[29] — clippedMean (Kırpılmış ortalama) 
### Tanım: 
> Aşırı uç değerler çıkarıldıktan sonra hesaplanan ortalama. 
### 💊 Proses Yorumu:
- ✔ **Spike veya gürültülü sensörlerde daha güvenilir ortalama sağlar.** 
- ✔ **Gerçek operasyon seviyesini outlier’lardan bağımsız gösterir.** 
> ## 📌 fv[30] — clippedStd (Kırpılmış Standart Sapma)
### Tanım
Aşırı uç değerler (outlier) çıkarıldıktan sonra hesaplanan standart sapmadır.  
Sistemdeki gerçek yayılımı, spike veya bozuk sensör okumalarından etkilenmeden ölçer.
## PLC Yorumu
- ✔ **Gerçek operasyonel gürültüyü ölçer** — sensör bozulduğunda, kablo temassızlığında veya spike içeren verilerde bile stabil yayılım değeri sağlar.
- ✔ **Outlier’ları yok saydığı için daha kararlı bir metrik sunar** — özellikle titreşim, akım ve basınç sinyallerinin standard sapmasının yanıltıcı olmasını engeller.
## Neden Kullanılır?
- **Gürültülü sensörlerde:**
  std değeri hatalı büyüyebilir, clippedStd ise yalnızca gerçek çalışma bölgesindeki varyansı ölçer.
- **Makine titreşim analizinde:**
  Ani mekanik darbeler veya sensör spike'ları rms/std hesaplarını bozarken clippedStd gerçek titreşim seviyesini korur.
- **PID tuning ve proses izleme için:**
  Gürültü seviyesinin daha doğru ölçülmesini sağlar → daha sağlam kontrol parametreleri seçilebilir.
> ## 📌 fv[31] — uniqueFrac (Benzersiz değer oranı) 
### Tanım: 
> uniqueCount / n — Sinyaldeki benzersiz değer oranı. 
### 💊 Proses Yorumu:
- ✔ **Düşük uniqueFrac → Sinyal adım adım, düşük çözünürlükte veya quantize.** 
- ✔ **Yüksek uniqueFrac → Daha analog ve sürekli değişen yapı.** 
- ✔ **Sensör çözünürlüğü, ADC bit değeri, PWM davranışları ile ilişkilidir.** 
> ## 📌 fv[32] — slope (Doğrusal Trend Eğimi) 
### Tanım: 
> Zaman serisinin doğrusal trendinin eğimi. 
- ✔ **Pozitif slope → yükselen trend** 
- ✔ **Negatif slope → düşen trend** 
### 💊 Proses Yorumu:
- ✔ **Sıcaklık, akım, basınç vb. verilerde drift, yavaş tırmanma, yavaş çökme tespitinde çok kritiktir.** 
- ✔ **slope sürekli pozitif → süreç ısınıyor, yük artıyor, sürtünme artıyor.** 
- ✔ **slope sürekli negatif → soğuma, basınç kaybı, akım düşmesi, güç zayıflığı.** 
- ✔ **Vibrasyonda düşük slope beklenir → artıyorsa yatak bozulması gibi trend başlıyor olabilir.** 
> ## 📌 fv[33] — intercept (Trend’in Y-eksenini Kesim Noktası) 
### Tanım: Trend çizgisinin 0. zamandaki tahmini değeri. 
### 💊 Proses Yorumu:
- ✔ **Tek başına teşhis amacıyla çok kullanılmaz, daha çok slope ile birlikte trendin seviyesini anlamak için.** 
- ✔ **Sistem startup, basınç offset, sensör offset incelemelerinde anlamlı olabilir.** 
> ## 📌 fv[34] — r2 (R-Squared, Trend Uygunluk Katsayısı) 
### Tanım: 
> Doğrusal modelin (slope–intercept) veriyi ne kadar iyi açıkladığını ölçer. 
- ✔ **1’e yakın → güçlü doğrusal trend** 
- ✔ **0’a yakın → çok gürültülü veya doğrusal olmayan sinyal** 
### 💊 Proses Yorumu:
- ✔ **r2 yüksek → süreç düzgün bir şekilde belirli bir yöne ilerliyor (ısı artışı, basınç yükselmesi).** 
- ✔ **r2 düşük → sinyal kaotik, titreşimli, dalgalı, trend yok.** 
- ✔ **PID döngüsü salınıyorsa r2 düşer.** 
- ✔ **Mekanik vibrasyonlarda r2 genelde düşüktür → kaotik doğal davranış.** 
> ## 📌 fv[35] — diffMean (Birinci türev ortalaması) 
### Tanım: 
> Ardışık örnekler arasındaki farkların ortalaması. 
- ✔ **Aslında sinyalin ortalama hızını ölçer.** 
### 💊 Proses Yorumu:
- ✔ **Pozitif diffMean → sinyal genel olarak yukarı gidiyor.** 
- ✔ **Negatif → aşağı gidiyor.** 
- ✔ **slope ile kıyaslanabilir, fakat slope daha global trendken diffMean daha lokal hareketi yansıtır.** 
- ✔ **Kontrol döngüsünde aşırı osilasyon varsa diffMean 0’a yakın olur.** 
> ## 📌 fv[36] — diffStd (Birinci türev standart sapması) 
### Tanım: Değişimin dalgalanma miktarı. 
### 💊 Proses Yorumu:
- ✔ **Yüksek diffStd → sinyal çok oynak, hızlı değişiyor.** 
- ✔ **Motor akımı diffStd artarsa → yük dalgalanıyor veya mekanik sıkışma var.** 
- ✔ **Sıcaklık gibi yavaş değişen sinyallerde diffStd düşük olmalı → artış anomali göstergesi.** 
> ## 📌 fv[37] — diffRms (Birinci türev RMS) 
### Tanım: 
> Zaman serisinin değişim hızının enerji benzeri ölçüsü. 
### 💊 Proses Yorumu:
- ✔ **Vibrasyon ve akım harmoniklerinde diffRms kritik bir göstergedir.** 
- ✔ **Sinyalin “hareketlilik enerjisini” verir.** 
- ✔ **diffRms artıyorsa → mekanik stres, bearing bozulması, ani yük değişimi olabilir.** 
> ## 📌 fv[38] — posDiff / n (Pozitif değişim oranı) 
> ## 📌 fv[39] — negDiff / n (Negatif değişim oranı) 
### Tanım: 
> Ardışık farkların yukarı yönlü ve aşağı yönlü sıklığı. 
### 💊 Proses Yorumu:
- ✔ **posDiff oranı baskın → sinyal çoğunlukla yükseliyor.** 
- ✔ **negDiff oranı baskın → sinyal çoğunlukla düşüyor.** 
- ✔ **İki oran eşit → sinyal dengeli, osilasyonlu veya kararlı olabilir.** 
- ✔ **Vibrasyon sinyallerinde oranlar genelde eşit olur; dengesizlik varsa sensör bias’ı olabilir.** 
> ## 📌 fv[40] — zeroCross (Sıfır geçiş sayısı) 
### Tanım: 
> Sinyal işaret değiştirirken kaç kez 0 eksenini kestiği. 
### 💊 Proses Yorumu:
- ✔ **Titreşim, akım ve hız ölçümlerinde çok önemli bir göstergedir.** 
- ✔ **Yüksek zeroCross → yüksek frekanslı bileşenler mevcut.** 
- ✔ **Az zeroCross → düşük frekans veya DC ağırlıklı sinyal.** 
- ✔ **Ani değişim → mekanik gevşeme, rezonans veya filtre bozulması göstergesi olabilir.** 
> ## 📌 fv[41] — zeroCrossRate (Zaman başına sıfır geçiş frekansı) 
### Tanım: 
> zeroCross / pencere uzunluğu 
### 💊 Proses Yorumu:
- ✔ **Sinyalin frekansını kabaca tahmin etmek için kullanılabilir.** 
- ✔ **Frekans artışı → vibrasyon şiddetleniyor, tahrik osilasyonu artıyor.** 
- ✔ **Frekans düşüşü → sistem yavaşlıyor, damping artıyor, sürtünme yükselmiş olabilir.** 
> ## 📌 fv[42] — maxDiff (En büyük ardışık artış) 
> ## 📌 fv[43] — minDiff (En büyük ardışık düşüş) 
### Tanım: 
> Örnekler arasındaki en büyük yükseliş ve en büyük düşüş. 
### 💊 Proses Yorumu:
- ✔ **Sensörde ani sıçrama, basınçta ani çökme, akımda tepki tepe noktası → maxDiff / minDiff ile yakalanır.** 
- ✔ **Peak-to-peak değişimler için kritik sinyal.** 
- ✔ **Mekanik şok veya tork darbesi varsa fark değerleri bir anda büyür.** 
> ## 📌 fv[44] — jerkRms (İkinci türev RMS – Jerk enerjisi) 
### Tanım: 
> Jerk = ivmenin türevi. Sinyalin üçüncü derece değişim hızını verir. 
PLC sinyali için → “ani değişimlerin keskinliği”. 
### 💊 Proses Yorumu:
- ✔ **Jerk arttıkça sinyal daha sert, daha keskin değişiyor → mekanik şok, çarpma, dişli boşluğu, motor kontrol problemleri.** 
- ✔ **Vibrasyon analizinde jerk, özellikle boşluk (backlash) veya gevşek bağlantı tespitinde çok güçlü bir metriktir.** 
- ✔ **Sıcaklık gibi yavaş sinyallerde jerkRms düşük olmalı → artıyorsa sensör gürültüsü veya arızası.** 
> ## 📌 fv[45] — peakToPeakDiff (diff’in tepe-çukur farkı) 
### Tanım: maxDiff - minDiff 
> Ardışık değişimlerin en büyüğü ve en küçüğü arasındaki fark. 
### 💊 Proses Yorumu:
- ✔ **“Değişimin değişimi” yani sinyalin ne kadar agresif sallandığını gösterir.** 
- ✔ **peakToPeakDiff yükseliyorsa → proses sallanıyor, tork dalgalanıyor, akım kararsız, PID loop overshoot yapıyor.** 
- ✔ **Vibrasyon ve tork ölçümlerinde güçlü bir uyarı göstergesidir.** 
> ## 📌 fv[46] — trendCurvature  
### Tanım: 
> Zaman serisinin trendindeki ivmeyi, yani hızlanma / yavaşlama derecesini ölçmek.
> Doğrusal trend eğimleri bunu yakalayamaz — ama kıvrımlılık yakalar.
> İkinci dereceden polinom trend eğrisi üzerinden kıvrımlılık ölçülür. 
> Hızlanan / yavaşlayan trendlerin tespitine yarar.. 
### 💊 Proses Yorumu:
- ✔ **Dönüş (reversal) noktalarını yakalar**
- ✔ **Bollinger/RSI gibi indikatörlerin göremediği ivmeyi algılar**
- ✔ **Trend kırılması öncesi uyarı sinyali üretir**
- ✔ **Trend gücü + yön değişimi kombinasyonunu tek bir sayı ile verir**
- ✔ **Noise’a karşı dayanıklıdır (sadece farkların aralığına bakar)**
> ## 📌 fv[47] — stabilityScore (Stabilite Skoru) 
### Tanım: 
> Genelde farklı istatistiklerin birleşimiyle hesaplanan normalize bir kararlılık metriği. 
> Sinyalin stabil olup olmadığını 0–1 aralığında değerlendiren bir değer. 
### 💊 Proses Yorumu: (🚨 çok önemli): 
- ✔ **1 → tamamen stabil, düzgün, dalgalanma düşük** 
- ✔ **0 → çok düzensiz, gürültülü, kontrolsüz** 
> StabilityScore genellikle şunları içerir: 
- düşük varyans 
- düşük diffStd / diffRms 
- düşük jerk 
- yüksek r2 (trend düzgün ise) 
- düşük peak-to-peak 
- düşük entropy 
- ✔ **Bu metrik “bir bakışta stabilite ölçümü” sağlar. 
- ✔ **Makine durumu, proses kararlılığı, PID tuning kalitesi gibi alanlarda çok değerlidir. 
> ## 📌 fv[48] — emaSlow[n-1] (Yavaş EMA Son Değer) 
### Tanım: 
> Uzun periyotlu EMA’nın en son hesaplanan değeri. 
> Sinyalin uzun vadeli trendini gösterir. 
### 💊 Proses Yorumu:
- ✔ **Isı, basınç, akım gibi yavaş değişen proseslerde baz çizgi (baseline) olarak kullanılabilir.** 
- ✔ **Sinyal emaSlow’un çok üzerine çıkıyorsa → olası ani yük artışı veya geçici anomali**
- ✔ **Sürekli uzaklaşıyorsa → kalıcı trend değişikliğinin işareti** 
> ## 📌 fv[49] — emaFast[n-1] (Hızlı EMA Son Değer) 
### Tanım: Daha kısa pencereli EMA’nın son değeri. 
### 💊 Proses Yorumu:
- ✔ **Anlık değişimlere duyarlıdır.** 
- ✔ **emaFast – emaSlow farkı, bir nevi momentum veya kısa vadeli trend gücünü gösterir.** 
- ✔ **PID osilasyonları veya yüksek frekanslı titreşimleri emaFast yakalar.** 
> ## 📌 fv[50] — resSlowMean (Slow EMA Artık Ortalaması) 
Artık = (sinyal – emaSlow) 
### Tanım: 
> Uzun vadeli trendden sapmanın ortalaması. 
### 💊 Proses Yorumu:
- ✔ **Sinyal ortalama olarak baz çizginin üzerinde mi altında mı?** 
- ✔ **Sıcaklık sürekli emaSlow’un üzerinde → sistem ısınıyor.** 
- ✔ **Akım sürekli altında → yük hafiflemiş.** 
- ✔ **Basınç sürekli altında → kaçak olabilir.** 
> ## 📌 fv[51] — resSlowStd (Slow Residual Standart Sapması) 
### Tanım: 
> Uzun vadeli trend etrafındaki oynaklığın miktarı. 
### 💊 Proses Yorumu:
- ✔ **Basınç veya motor akımı gibi sinyallerde resSlowStd beklenenden yüksekse:** 
- 📢 Proses dalgalı 
- 📢 Regülasyon kötü 
- 📢 PID parametreleri yetersiz 
- 📢 Mekanik çalkantı olabilir 
> ## 📌 fv[52] — resFastMean (Fast EMA Artık Ortalaması) 
### Tanım: Hızlı trendden (emaFast) sapmanın ortalaması. 
### 💊 Proses Yorumu:
- ✔ **Daha lokal bias gösterir.** 
- ✔ **Sensör kısa süreli kayma yapmış mı?** 
- ✔ **Hafif titreşimli sinyallerde dengesizliği yakalar.** 
- ✔ **Yükselen/azalan ivme dönemlerinde anlamlıdır.** 
> ## 📌 fv[53] — resFastStd (Fast EMA Artık Std) 
### Tanım: 
> Hızlı EMA’ya göre oynaklık. 
### 💊 Proses Yorumu:
- ✔ **Sinyalin kısa vadede ne kadar çalkantılı olduğunu ölçer.** 
- ✔ **Motor akımı, vibrasyon, hız kontrolü gibi kısa zaman sabitli süreçlerde önemlidir.** 
- ✔ **Artış → osilasyon, gevşeklik, rezonans, yük dengesizliği.** 
> ## 📌 fv[54] — resSlowRms (Slow EMA Artık RMS) 
### Tanım: 
> Uzun trendden sapmanın enerji değeri. 
### 💊 Proses Yorumu:
- ✔ **Proses ne kadar stabil?** 
- ✔ **resSlowRms düşük → proses düzgün ve sakin.** 
- ✔ **yüksek → makine agresif çalışıyor, akım basınç dalgalanıyor.** 
> ## 📌 fv[55] — resFastRms (Fast EMA Artık RMS) 
### Tanım: 
> Hızlı trend sapmalarının enerjisi. 
### 💊 Proses Yorumu:
- ✔ **Yüksek frekanslı gürültü, harmonik, jitter tespitinde etkili.** 
- ✔ **Vibrasyon analitiğinde ince sinyali yakalar.** 
- ✔ **PID kaynaklı küçük hızlı dalgalanmalar burada görünür.** 
> ## 📌 fv[56] — slowSpikeCount (Slow EMA’ya Göre Spike Sayısı) 
### Tanım: 
> Spike = |resSlow| > threshold 
### 💊 Proses Yorumu:
- ✔ **Uzun vadeli beklentiye göre anormal yükselmeler/düşüşler 
- ✔ **Proses anlık şoklara maruz kalıyor olabilir: 
- ✔ **tork darbesi 
- ✔ **basınç reseti 
- ✔ **akım sıçraması 
- ✔ **ani ısı değişimi 
> ## 📌 fv[57] — fastSpikeCount (Fast EMA’ya Göre Spike Sayısı) 
### Tanım: Daha kısa vadeli spike sayısı. 
### 💊 Proses Yorumu:
- ✔ **Yüksek frekanslı anomali tespiti 
- ✔ **Vibrasyon pikleri 
- ✔ **Sensör jitter 
- ✔ **Gürültü altında çalışan sistemlerde hızlı spike sayısı kritik. 
> ## 📌 fv[58] — slowSpikeRate (Spike / n) 
### Tanım: Uzun vadeli anomali oranı. 
### 💊 Proses Yorumu:
- ✔ **Makinenin genel stabilite ölçüsü 
- ✔ **Sürekli slow spike oluşması → proses çok dalgalı veya mekanik parça gevşek. 
> ## 📌 fv[59] — fastSpikeRate (Fast Spike / n) 
### Tanım: Kısa vadeli anomali frekansı. 
### 💊 Proses Yorumu:
- ✔ **Yüksek fastSpikeRate → 
- ✔ **vibrasyon artışı 
- ✔ **sensör gürültüsü 
- ✔ **PID high-frequency osilasyon 
- ✔ **bearing/dişli bozulmalarına işaret edebilir. 
> ## 📌 fv[60] — lastValue (Son Örnek) 
### 💊 Proses Yorumu:
- ✔ **Anlık değer. 
- ✔ **Trend karşılaştırmalarında referans alınır. 
- ✔ **Model son anda sinyalin nereye geldiğini bilmezse yorum hatalı olabilir. Bu yüzden çok önemli. 
> ## 📌 fv[61] — firstValue (İlk Örnek) 
### 💊 Proses Yorumu:
- ✔ **Pencere başındaki sistem durumunu gösterir. 
- ✔ **last - first ile bölgedeki değişimin net yönü alınır. 
> ## 📌 fv[62] — lastMinusFirst (Toplam Değişim) 
### Tanım: Sinyalin pencere boyunca yaptığı net hareket. 
### 💊 Proses Yorumu:
- ✔ **Pozitif → süreç yükselmiş 
- ✔ **Negatif → süreç azalmış 
- ✔ **Sıcaklık/akım/başınç drift tespitinde çok kritik. 
> ## 📌 fv[63] — emaSlowMinusMean (Slow EMA – Basit Ortalama Farkı) 
### Tanım: EMA uzun trendi ile pencerenin ortalama değeri arasındaki fark. 
### 💊 Proses Yorumu:
- ✔ **Eğer emaSlow > mean → sinyal yukarı doğru ivmeleniyor. 
- ✔ **emaSlow < mean → sinyal aşağı yönlü. 
- ✔ **“EMA → adaptif ortalama” ile “mean → statik ortalama” farkı makinenin trend hızını gösterir. 
- ✔ **Segment Yapısı 
n örneklik pencere şu şekilde 4’e bölünüyor: 
Segment 0: Başlangıç kısmı (0–25%) 
Segment 1: Erken orta bölüm (25–50%) 
Segment 2: Geç orta bölüm (50–75%) 
Segment 3: Son bölüm (75–100%) 
Her segment için mean, std, slope ve RMS çıkarılıyor. 
Bu yapılar özellikle trend değişikliği, geçişler, süreç stabilitesi ve anlık olay tespiti için son derece güçlüdür. 
> ## 📌 fv[64–67] = segMeans[0–3] 
Segment Ortalama Değerleri 
### Tanım: 
Her alt segmentin ortalama değeri. 
### 💊 Proses Yorumu:
Bu değerler sinyalin zaman içinde nasıl kaydığını anlamayı sağlar. 
Örnek yorumlamalar: 
segMean[0] < segMean[3] → süreç yükseliyor (ısı artıyor, akım yükseliyor) 
segMean[0] > segMean[3] → süreç düşüyor 
segmentler arası ani sıçramalar → proses içinde geçiş/bozulma/ayar değişimi 
Segment ortalamalarının çok farklı olması → proses sabit değil, değişken. 
Özellikle: 
Motor akımında segment 3 çok yüksek → son kısımda yük artmış 
Basınç segment 0–1 düşük, 2–3 yüksek → valf geç açılıyor olabilir 
Sıcaklık segment 0 → soğuk, segment 3 → ısınma eğilimi 
> ## 📌 fv[68–71] = segStds[0–3] 
Segment Standart Sapmaları (Oynaklık) 
### Tanım: 
Her segmentte sinyal ne kadar dalgalı. 
### 💊 Proses Yorumu:
segStd düşük → segment stabil 
segStd yüksek → segmentte gürültü, titreşim, dengesizlik, PID çalkantısı 
Seg1 düşük, Seg2 yüksek → süreç bu bölgede bozulmaya başlıyor 
Uygulama örnekleri: 
Motor akımı: son segmentte yüksek std → mekanik sürtünme artmaya başlıyor 
Basınç: segment 2–3 std yükseliyor → valf açıldığında sistem kontrolsüz 
Segment STD özellikle “lokal arıza başlangıcı” tespitinde çok güçlüdür. 
> ## 📌 fv[72–75] = segSlopes[0–3] 
Segment Trend Eğimi (Slope) 
### Tanım: 
Her segmentte lineer fit eğimi. 
### 💊 Proses Yorumu:
Pozitif slope → segmentte yükselen trend 
Negatif slope → segmentte düşen trend 
0’a yakın → sabit bölüm 
Özellikler: 
segSlope[0] ≈ 0, segSlope[3] >> 0 → pencere sonunda ani yükseliş 
segSlope sıfırdan sıfıra kayıyor → süreç kademeli şekilde düzleşiyor 
Segment 1 veya 2’de anomali → proses ortasında bozulma var 
Uygulama örnekleri: 
Sıcaklık son segment slope > 0 → sensör ısınmaya devam ediyor 
Akım segment 1–2 slope pozitif → yük binmeye başlıyor 
Basınç segment 3 slope negatif → sistem rahatlıyor 
> ## 📌 fv[76–79] = segRms[0–3] 
Segment Enerji / Güç (RMS) 
### Tanım: 
Her segmentte sinyal RMS değeri. 
### 💊 Proses Yorumu:
RMS, titreşim veya akım gibi sinyallerde enerji/art yükünü temsil eder. 
RMS yüksek → yük yüksek 
RMS düşük → stabil, sakin çalışma 
Özellikle mekanik sistemlerde kritik: 
segRms[0] < segRms[3] → yük zaman içinde artıyor 
segRms[2] çok yüksek → orta bölgede aşırı vibrasyon 
segRms segmentler arası ani sıçrıyorsa → mekanik gevşeme, bearing bozulması 
Basınç ve sıcaklık için: 
RMS yükseliyorsa → kontrol zayıf, osilasyon artıyor. 
> ## 📌 fv[80] = segMeanDelta10 
Segment1 – Segment0 Ortalama Farkı 
Anlamı: İlk iki segment arasındaki ortalama değişimi ölçer. 
### 💊 Proses Yorumu:
Pozitif → süreç ikinci bölümde yükselmeye başlıyor 
Negatif → düşmeye başlıyor 
Büyük fark → ani geçiş, ani yük/akım/ısı değişimi 
> ## 📌 fv[81] = segMeanDelta21 
Segment2 – Segment1 Ortalama Farkı 
Anlamı: Orta bölgede trend değişimini gösterir. 
### 💊 Proses Yorumu:
Bu değer genelde proses ortasında yaşanan değişimleri anlamak için en kritiklerden biridir. 
> ## 📌 fv[82] = segMeanDelta32 
Segment3 – Segment2 Ortalama Farkı 
Anlamı: Son bölümdeki değişim. 
### 💊 Proses Yorumu:
Segment3 ortalaması yüksek → pencere sonunda kısa süreli sıçrama 
Düşük → süreç kapanıyor/gevşiyor 
Son bölüm anomaly check için çok değerlidir. 
> ## 📌 fv[83] = segMeanDelta30 
Segment3 – Segment0 Ortalama Farkı (Uzun Trend) 
Bu, segmentlerin baştan sona genel drift’ini gösterir. 
### 💊 Proses Yorumu:
Pozitif → uzun vadeli yükselme 
Negatif → uzun vadeli düşüş 
Sıfıra yakın → genel olarak stabil 
Bu değer, trend yönü için tek başına çok kuvvetlidir. 
> ## 📌 fv[84] = segStdMax 
Segmentler Arası En Büyük Std 
Anlamı: En dalgalı segmentin standart sapması. 
### 💊 Proses Yorumu:
Prosesin en problemli yerini söyler: 
segStdMax yüksek → o bölgede gürültü/titreşim/PID çalkantısı artmış 
> ## 📌 fv[85] = segStdMin 
Segmentler Arası En Küçük Std 
Anlamı: En stabil segment. 
PLC açısından: 
Sistem hangi bölümde en stabil çalışıyor → buradan anlaşılır 
> ## 📌 fv[86] = segStdRange 
segStdMax − segStdMin 
Anlamı: Segmentler arasındaki dalgalanma farkı. 
### 💊 Proses Yorumu:
Büyük fark → proses bazı yerlerde çok kararsız 
Küçük fark → tüm pencere boyunca benzer stabilite 
Bu “stabilite uniformity” ölçüsüdür. 
> ## 📌 fv[87] = largestMeanJump 
Segmentler Arası En Büyük Ortalama Sıçraması 
Örnek: 
max(|mean0-mean1|, |mean1-mean2|, |mean2-mean3|) 
### 💊 Proses Yorumu:
Süreçteki en büyük ani değişim burada. 
Bu genelde: 
Motor yük değişimi 
Basınç valfi ani açılması 
PID setpoint step 
Sıcaklık direnç ani tetiklenmesi 
gibi olayları yakalar. 
> ## 📌 fv[88] = segMeanSlope 
Segment Ortalamalarının Eğimi 
Segment mean dizisi → [m0, m1, m2, m3] üzerine lineer fit slope. 
### 💊 Proses Yorumu:
Pozitif → segment ortalamaları zamanla artıyor → yükselen trend 
Negatif → düşen trend 
Sıfıra yakın → genel olarak yatay/stabil 
Bu, uzun segment trendinin sade bir temsilidir. 
> ## 📌 fv[89] = segStdSlope 
Segment STD’lerinin Eğimi 
STD dizisi [s0, s1, s2, s3] üzerine lineer fit. 
### 💊 Proses Yorumu:
Pozitif → her segmentte oynaklık artıyor → sistem bozuluyor 
Negatif → süreç zamanla toparlıyor 
0 → stabilite sabit 
İşlem istikrarını özetleyen çok değerli bir metriktir. 
> ## 📌 fv[90] = segMeanVar 
Segment Ortalamalarının Varyansı 
SegMean değerleri arasındaki dağılımın genişliği. 
### 💊 Proses Yorumu:
Büyük varyans → segment ortalamaları birbirinden çok farklı → uniform değil 
Küçük varyans → süreç genel olarak düz çizgi gibi 
Bu, “genel davranış değişkenliği” için iyi bir metriktir. 
> ## 📌 fv[91] = segStdVar 
Segment Standart Sapmalarının Varyansı 
STD’lerin dağılımının genişliğini ölçer. 
### 💊 Proses Yorumu:
Büyük varyans → bazı segmentlerde gürültü çok yüksek 
Küçük varyans → sistem tüm zaman boyunca aynı stabilitede 
> ## 📌 fv[92] = segSlopeMax 
Segment Trendlerinin Maksimum Değeri 
Slope dizisi → [slope0, slope1, slope2, slope3] 
### 💊 Proses Yorumu:
En hızlı yükselişin olduğu segmenti temsil eder. 
Örneğin segment 3 yükseliyorsa son anlarda ani artış var. 
> ## 📌 fv[93] = segSlopeMin 
Segment Trendlerinin Minimumu 
Bu, en hızlı düşüşün olduğu bölgeyi söyler. 
Örneğin segment 1’de çok negatif slope → orta bölgede keskin düşüş. 
> ## 📌 fv[94] = segSlopeRange 
segSlopeMax − segSlopeMin 
Anlamı: Segment eğimleri arasındaki dağılım. 
### 💊 Proses Yorumu:
Büyük fark → bazı segmentlerde yükseliş, bazılarında düşüş → süreç uniform değil 
Küçük fark → trend her yerde benzer → stabil sistem 
> ## 📌 fv[95] = segSlopeRms 
Segment Eğimi RMS (Trend Gücü) 
Slope değerlerinin enerji/şiddet ölçüsü. 
### 💊 Proses Yorumu:
Yüksek → segmentler genel olarak güçlü trend içeriyor (ani artış/azalış) 
Düşük → zaman boyunca trend zayıf, sistem sabit 
Titreşim veya akım sistemlerinde “trend gücü” için çok anlamlıdır. 
> ## 📌 fv[96] = totalEnergy 
Frekans Spektrumunun Toplam Enerjisi 
Anlamı: FFT binlerinin enerji toplamı. 
### 💊 Proses Yorumu:
Yüksek → sistemde yüksek titreşim, osilasyon, gürültü 
Düşük → stabil ve sakin çalışma 
Ani artış → mekanik gevşeme, rulman bozulması, PID kararsızlığı 
> ## 📌 fv[97] = centroid 
Spektral Kütle Merkezi (Spectral Centroid) 
Anlamı: Enerjinin frekans ekseninde ağırlık merkezi. 
### 💊 Proses Yorumu:
Düşük centroid → enerji düşük frekanslarda 
Yüksek centroid → sistem yüksek frekans bileşenlerine kayıyor (titreşim artışı, mekanik sürtünme) 
Bu metrik, “ses parlaklığı” gibi düşünülebilir ama mekanik sinyallerde titreşim dağılımını gösterir. 
> ## 📌 fv[98] = spread 
Spektral Yayılım 
Anlamı: Enerji ne kadar geniş bir frekans aralığına dağılmış. 
### 💊 Proses Yorumu:
Yüksek → sinyal geniş bantlı (gürültü artmış) 
Düşük → sinyal dar bantlı (motor nominal çalışıyor) 
Arıza oluştuğunda spread genelde artar. 
> ## 📌 fv[99] = flatness 
Spektral Düzlük (Spectral Flatness) 
(Sinyalin gürültü mü, ton mu olduğunu ölçer) 
### 💊 Proses Yorumu:
1’e yakın → beyaz gürültü gibi; ton yok → mekanik bozukluk, sürtünme, dağınık titreşim 
0’a yakın → belirgin tonlar → fan, motor, rulman karakteristik frekansları 
> ## 📌 fv[100] = crest
### Tanım: 
> Crest Factor (Peak / RMS) 
### 💊 Proses Yorumu:
- ✔ **Yüksek crest → kısa süreli darbeler, çarpma, rulman bozukluğu** 
- ✔ **Düşük crest → pürüzsüz çalışma** 
- ✔ **Bu arıza tespitinde çok kritik bir metriktir** 
> ## 📌 fv[101]–> ## 📌 fv[108] = bandEnergy[0..7]
### Tanım:
> 8 Bantlık Enerji Dağılımı
### 💊 Proses Yorumu:
- ✔ **Tipik olarak bantlar:** 
- ✔ **0: DC – düşük frekans** 
- ✔ **1–3: düşük/orta titreşim** 
- ✔ **4–7: yüksek frekans, harmonikler, rulman hasar frekansları** 
### 💊 Proses Yorumu:
- ✔ **Düşük bantlar ↑ → yük salınımı, PID osilasyonu** 
- ✔ **Orta bantlar ↑ → rezonans, mekanik dengesizlik** 
- ✔ **Yüksek bantlar ↑ → rulman iç/orta/dış bilezik hasarı, sürtünme, gövde rezonansı** 
- ✔ **Bu dağılım bir çeşit “frekans fingerprint” oluşturur.** 
> ## 📌 fv[109] = domFreq 
### Tanım:
> Baskın Frekans (Dominant Frequency): Sinyalde en yüksek enerjiyi taşıyan frekans. 
### 💊 Proses Yorumu:
- ✔ **Motor hızına yakın → normal**
- ✔ **2×, 3× harmonikler → dengesizlik** 
- ✔ **Yüksek frekanslarda ani kayma → anomali** 
- ✔ **Tek başına çok güçlü bir arıza belirtecidir.** 
> ## 📌 fv[110] = sqrt(domMag)
### Tanım:
> Baskın Frekansın Kök-Enerjisi: Baskın frekans büyüklüğünü normalize eder. 
### 💊 Proses Yorumu:
- ✔ **Artışı → domFreq enerjisi yükseliyor (özellikle rulman problemlerinde kritik)** 
> ## 📌 fv[111] = secondFreq 
### Tanım:
> İkinci Baskın Frekans: İlk dominanta ek olarak 2. büyük pik. 
### 💊 Proses Yorumu:
- ✔ **Harmonik çiftler → rezonans modları** 
- ✔ **İkinci frekansta yükseliş → mekanik arızalarda genelde birlikte büyür** 
> ## 📌 fv[112] = harmonicity 
### Tanım:
> Harmonik Üst-Alt Uyum Ölçüsü: Enerji harmonik frekanslarda mı yoğunlaşıyor? 
### 💊 Proses Yorumu:
- ✔ **Yüksek → sistem harmonik olarak çalışıyor (motor/generator normal)** 
- ✔ **Düşük → enerjinin harmonik yapısı bozulmuş → arıza işareti** 
> ## 📌 fv[113] = lowRatio
### Tanım:
> Düşük Bant Enerjisi / Toplam Enerji: Düşük frekans oranı.
### 💊 Proses Yorumu: 
- ✔ **Genellikle yük salınımları veya yavaş osilasyonları gösterir.** 
> ## 📌 fv[114] = midRatio
### Tanım:
> Orta Bant Enerjisi / Toplam Enerji
### 💊 Proses Yorumu:
- ✔ **Orta frekansta enerji yoğunlaşması genelde:** 
- ✔ **Rezonans** 
- ✔ **Mil hizalama bozukluğu** 
- ✔ **Mekanik gevşeme** 
> ## 📌 fv[115] = highRatio 
### Tanım:
> Yüksek Bant Enerjisi / Toplam Enerji 
✔ **Yüksek frekanslar:**
- ✔ **Rulman hasarı** 
- ✔ **Dişli vuruntuları** 
- ✔ **Sürtünme** 
- ✔ **Mekanik temas** 
- ✔ **gibi arızaları belirgin gösterir.** 
> ## 📌 fv[116] = specEntropy 
### Tanım:
> Spektral Entropi: Spektrum ne kadar düzensiz? 
### 💊 Proses Yorumu:
✔ **Yüksek → gürültü çok, süreç karmaşık**
✔ **Düşük → belirgin frekanslar baskın, sistem düzenli** 
> ## 📌 fv[117] = noiseFloor 
### Tanım:
> Spektral Gürültü Tabanı: FFT’de taban gürültü seviyesi. 
### 💊 Proses Yorumu:
✔ **Artması → yatak aşınması, sürtünme, sensör bozulması** 
> ## 📌 fv[118] = snr 
### Tanım:
> Sinyal Gürültü Oranı (Signal-to-Noise Ratio) 
### 💊 Proses Yorumu:
- ✔ **Yüksek → sistem net, stabil** 
- ✔ **Düşük → gürültü artmış, sistem bozuluyor** 
- ✔ **Motor ve fan gibi sistemlerde SNR düşüşü erken uyarıdır.** 
> ## 📌 fv[119] = dcComponent 
### Tanım:
> DC Bileşeni (Offset): Sinyalin ortalama kayması. 
### 💊 Proses Yorumu:
- ✔ **Setpoint drift** 
- ✔ **PID bias** 
- ✔ **Basınç/sıcaklık offset kayması** 
- ✔ **DC kayması çoğunlukla kalibrasyon veya mekanik sürtünmenin erken işaretidir.** 
> ## 📌 fv[120] = acEnergyRatio 
### Tanım:
> AC Enerjisi / Toplam Enerji: Salınım miktarı. 
### 💊 Proses Yorumu:
- ✔ **Yüksek → sistemde osilasyon baskın** 
- ✔ **Düşük → sistem daha stabil, az titreşimli** 
- ✔ **Bu değer PID tuning analizinde özellikle faydalıdır.** 
