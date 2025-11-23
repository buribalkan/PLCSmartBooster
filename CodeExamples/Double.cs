return Event?.Address?.Contains("MAIN_Simu.fBoxX(1)") == true
       && (Event.ValueAsDouble()?.CompareTo(100.0) > 0);
//Daha okunur iki alternatif:
var isThatTag = Event?.Address?.Contains("MAIN_Simu.fBoxX(1)") == true;
var v = Event.ValueAsDouble(); // double?
return isThatTag && v.HasValue && v.Value > 100.0;
//ve tek satıra sıkıştırılmış versiyon:
return Event?.Address?.Contains("MAIN_Simu.fBoxX(1)") == true
       && (Event.ValueAsDouble() ?? double.NegativeInfinity) > 100.0;
