Bu üç fonksiyon, PLC veya sistemden gelen son değerleri alıp farklı türlere dönüştürmek için kullanılır:

- `Last` → ham string değer döndürür
- `LastDouble` → değeri double'a çevirir
- `LastBool` → değeri bool'a çevirir

## 1) Last(string tag)

**Ne yapar?**

- `_last` sözlüğünden verilen `tag` için saklanan son değeri string olarak döndürür.
- Eğer `tag` boşsa, yoksa veya `_last` tanımlı değilse `null` döner.
- Değeri olduğu gibi döndürür, hiç dönüşüm yapmaz.

**Kısaca:**  
"Bu tag’in en son gelen string değerini bana ver."

## 2) LastDouble(string tag)

**Ne yapar?**

- Önce `Last(tag)` ile son string değeri alır.
- Bu değeri `Double(...)` fonksiyonu ile double'a dönüştürür.
- Sayı değilse `null` döndürür.

**Kısaca:**  
"Bu tag’in son değerini double olarak ver."

## 3) LastBool(string tag)

**Ne yapar?**

- Önce `Last(tag)` ile son string değeri alır.
- Bu string değeri `Bool(...)` fonksiyonu ile true/false'a çevirmeyi dener.
- "true", "false", "1", "0", "on", "off", "evet", "hayır" gibi değerleri destekler.
- Anlamıyorsa `null` döndürür.

**Kısaca:**  
"Bu tag’in son değerini mantıksal (bool) olarak ver."
