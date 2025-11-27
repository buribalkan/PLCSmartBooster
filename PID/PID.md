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
