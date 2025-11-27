# 📘 PID Kontrol Formülleri Tablosu

| 🔧 **Konu** | 🔢 **Formül** | 📌 **Açıklama** |
|------------|--------------|----------------|
| ⚡ **Hata (Error)** | `error = SP - PV` | Anlık hata değeri (Setpoint – Process Value). |
| 📏 **Mutlak Hata** | `absError = abs(error)` | Band veya stabilite kontrolünde kullanılır. |
| 🎯 **Hata Bandı (Within Band)** | `WithinBand = abs(SP - PV) <= Band` | PV, SP’ye yeterince yakın mı? |
| 🅿️ **P (Proportional) Terimi** | `P = Kp * error` | Hatanın büyüklüğüne doğrudan orantılı tepki. |
| 🟩 **I (Integral) Terimi** | `integral += error * dt` <br> `I = Ki * integral` | Hata zamanla birikerek ofseti düzeltir. |
| 🛑 **I Anti-windup** | `integral = Clamp(integral, Imin, Imax)` | Integralin taşmasını engeller. |
| 🔼 **D (Derivative) Terimi** | `derivative = (error - prevError) / dt` <br> `D = Kd * derivative` | Hatanın değişim hızına tepki verir. |
| 🧮 **PID Çıkışı** | `output = P + I + D` | PID toplam kontrol sinyali. |
| 📉 **Çıkış Limitleri** | `output = Clamp(output, OutMin, OutMax)` | Çıkışı fiziksel sınırlar içinde tutar. |
| ⏱️ **Settling (Yerleşme) Zamanı** | `if (WithinBand) counter++; else counter = 0` <br> `Settled = counter >= RequiredSamples` | Sistem belirli süre band içinde kalınca stabil kabul edilir. |
| 🔧 **PV Filtreleme (Opsiyonel)** | `filteredPV = filteredPV + α(PV - filteredPV)` | Gürültülü PV için düşük geçiren filtre (low-pass). |
| 🔇 **Derivative için PV kullanımı (Noise Reduction)** | `derivative = -(PV - prevPV) / dt` | Gürültüyü azaltmak için türevin PV üzerinden alınması. |



# PID Formülünün KÖK mantığı

PID çıkışı:


Burada:

- **Kp** = Proportional gain  
- **Ki** = Integral gain  
- **Kd** = Derivative gain  

Bunlar her bir terimin davranışını güçlendirir veya zayıflatır.

---

## ✔ 1) Integral neden Ki ile çarpılır?


Bu sadece **ham integral toplamıdır**.  
Bu değerin PID’e etkisi şu şekilde hesaplanır:


**Ki ne işe yarar?**

- Ki büyür → integral etkisi artar → ofset daha hızlı düzelir  
- Ki küçülür → integral etkisi yavaşlar → daha stabil ama daha yavaş sistem  

Integral → sistemin **uzun vadeli hatalarını düzeltir**.

---

## ✔ 2) Derivative neden Kd ile çarpılır?


Bu sadece **hata değişim hızıdır (slope)**.  
PID çıkışına etkisi:


**Kd ne işe yarar?**

- Kd büyür → sistem daha "öngörülü" olur → overshoot azalır  
- Kd küçülür → daha yumuşak ama daha yavaş tepki  

Derivative → hatanın **değişim hızını** ölçer.

---

## ✔ 3) Özet: Ki ve Kd olmadan PID çalışmaz

- Integral ham toplamdır → **Ki ile ağırlıklandırılır**  
- Derivative ham hızdır → **Kd ile ağırlıklandırılır**  

Yani:

- `integral += error * dt` tek başına PID değildir  
  → **PID katkısı: Ki × integral**

- `(error - prevError) / dt` tek başına türev değildir  
  → **PID katkısı: Kd × derivative**

---

## ✔ Tam PID formülü ilişkilendirilmiş şekilde

double error = SP - PV;

// P
double P = Kp * error;

// I
integral += error * dt;
double I = Ki * integral;

// D
double derivative = (error - previousError) / dt;
double D = Kd * derivative;

previousError = error;

// PID Output
double output = P + I + D;

❗ **integral ve derivative tek başına PID katkısı değildir**  
✔ **PID katkısı:**

- Integral → **Ki × integral**  
- Derivative → **Kd × derivative**  






