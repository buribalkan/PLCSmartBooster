// "(1)" -> "[1]" normalize
var addr = System.Text.RegularExpressions.Regex.Replace(Event?.Address ?? "", @"\((\d+)\)", "[$1]");
// Hedef tag mı?
var isThatTag = addr.Contains("MAIN_Simu.fBoxX[1]");
// Değeri double çek
var v = Event?.ValueAsDouble();
// Koşul
return isThatTag && v.HasValue && v.Value > 100.0;
