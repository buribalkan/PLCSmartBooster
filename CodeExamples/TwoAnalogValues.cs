//“A > 200 ve B > 190 ise alarm” nasıl yapılır?
//Bunu tek kuralda en temiz yapan yer: CombinationV3(çoklu tag koşulu).
//Aşağıdaki gibi kur:
//BoolTags: boş bırak(sadece analog bakacağız)
//AnalogTags:
//Tag = MAIN_Simu.fBoxX[1], Threshold = 200, GreaterThan = true
//Tag = MAIN_Simu.fBoxX[2], Threshold = 190, GreaterThan = true
//AnalogAnd = true(ikisi de sağlanmalı)
//Include: bu iki tag’i kapsayacak bir pattern (veya *); Exclude boş
//Bu kural “A ve B aynı anda eşik üstünde” durumunda hit üretir. (Biz CombinationV3’te son değerleri cache’ten okuyoruz; include’ı * tutmak en güvenlisi, ama istersem MAIN_Simu.fBoxX[*] de olur.)
//Neden Include/Exclude karışık hissettirdi?
//Include/Exclude bir ön filtre: O kural hangi event’leri dikkate alacak?
//Her kuralda ayrı tutuluyor ve o kuralın Observe’ında uygulanıyor.
//Örneğin AnalogRangeRule içine Matches() eklemiştik; CombinationV3, Chatter vs. de kendi içinde aynı mantıkla yapıyor.
//Alternatif: CustomScript
//Eğer özel mantık istersen (ör. tolerans, zaman penceresi) CustomScript ile de olur. Örn (bizde H’de son değer okuma varsa):
var a = H.LastDouble("MAIN_Simu.fBoxX[1]");
var b = H.LastDouble("MAIN_Simu.fBoxX[2]");
if (a.HasValue && b.HasValue && a.Value > 200 && b.Value > 190)
    return H.Hit($"A={a:F1} > 200 ve B={b:F1} > 190");
return false;
