# TrendCurvature (Eğri Kavisliği) Rehberi

TrendCurvature, lineer trendin “2. türevi” gibi çalışan; ivmelenme, hızlanma ve yön değişimi bilgisini veren güçlü bir metriktir.

Şunu anlamamızı sağlar:

- Yukarı yönlü trend artarak mı, azalarak mı ilerliyor?
- Aşağı yönlü trend hızlanarak mı, yavaşlayarak mı düşüyor?
- Trend dönüm noktasına mı gidiyor?
- Yakında bir "U-dönüş" var mı?
- Seri konkav mı, konveks mi?

Kullanım alanları: enerji, IoT, vibrasyon, proses kontrol, PLC davranışı, erken uyarı sistemleri.


---

## 1) TrendCurvature Nasıl Hesaplanır?

### **Basit yaklaşım:**
Lineer trend:
```

y ≈ m·t + b

```

### **2. derece polinomsal yaklaşım:**
```

y ≈ a·t² + b·t + c
curvature ≈ a

```

### **Alternatif: farkların farkı**
```

curv = mean( diff(diff(series)) )

```


---

## 2) “10 Değer” ile Neler Anlamlı Hale Gelir?

TrendCurvature kısa pencerede (örn. 10 değer) çok etkili sinyaller üretir.

---

### **SENARYO A — Setpoint değişimi → PV’nin tepkisi**

PID tuning için kritik davranış:

1. SP yükselir.
2. PV hızlanır → slope pozitif.
3. Yavaşlar → curvature negatif.
4. Tekrar hızlanıp overshoot yapabilir → curvature pozitif.

**Yorum:**

- `a > 0` → yükseliş hızlanıyor (overshoot riski)
- `a < 0` → sistem yavaşlıyor / plato
- `a` sürekli poz/neg değişiyorsa → osilasyon


---

### **SENARYO B — Vibrasyon: “bozuk rulman” imzası**

Bozuk rulman davranışı genelde lineer değildir → ivmeli artış yapar.

- RMS artmadan önce curvature artar.
- 10 örnek boyunca curvature pozitif → erken uyarı.
- Bozulma döngüsü: **pozitif → negatif → pozitif curvature**.


---

### **SENARYO C — Proses kırılma noktası (basınç/akış/sıcaklık)**

Stabil proseslerde:

- slope ≈ 0 olabilir
- curvature pozitifse → sistem yeni bir rejime geçiyor

Örneğin:

- Kazan basıncı sabit ama curvature + → yakında hızlı artış.
- Sıcaklık sabit ama curvature + → pompa debisi düşüyor olabilir.


---

### **SENARYO D — Sensör drift’i tespiti**

Sensör arızaları genelde kavislenerek başlar.

- Başta stabil
- Sonra yavaş drift
- 10 örneklik curvature bunu çok erken yakalar

**Yorum:**

- `a > 0` ve RMS düşük → sensör drift
- slope ≈ 0 fakat a > 0 → saf drift


---

### **SENARYO E — Pompa / kompresör yük eğrisi: “stall” yaklaşımı**

Stall öncesi:

- slope negatif
- curvature pozitif → düşüş hızlanıyor

10 örneklik curvature, stall sinyalini slope’tan **2–5 saniye önce verir**.


---

## 3) PLC Yorumu

Aşağıdaki tablo, curvature değerinin PLC açısından nasıl anlamlandırıldığını özetler.

| Durum | Curvature (a) | PLC Yorumu |
|------|----------------|-------------|
| Pozitif & büyüyor | eğri yukarı kıvrılıyor | PV hızlanıyor, overshoot riski, vibrasyon ivmeleniyor |
| Negatif & büyüyor | eğri aşağı kıvrılıyor | PV yavaşlıyor, stabilizasyon, basınç/akış düşüşünde hızlanma |
| Pozitif → Negatif → Pozitif | döngüsel kavis | osilasyon, mekanik gevşeme, agresif geri besleme |
| ~0 (düz) | düz çizgi | kararlı, inertia yüksek, PID yumuşak |
| slope ≈ 0 fakat curvature ≠ 0 | drift | sensör kayması, setpoint yaklaşımı |
| High | büyük ivme | transient, pompa yük değişimi, vibrasyon sıçraması |


---

## 4) 10 Örnek İçin İyi Cutoff Değerleri

Normalize edilmiş tipik curvature eşikleri:

```

|a| < 0.005       → tamamen stabil
0.005–0.02        → hafif kavis (normal)
0.02–0.05         → orta seviye, yaklaşan değişim
|a| ≥ 0.05        → ciddi kıvrılma (drift, kırılma, osilasyon)
|a| ≥ 0.10        → alarm seviyesi (rezonans / bozulma)

