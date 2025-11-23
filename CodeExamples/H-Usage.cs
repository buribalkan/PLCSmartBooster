// Analog sapma kontrolü
var v = H.Double(Event.Value);
if (H.IsBetween(v, 10, 20))
{
    H.Reason($"Değer aralıkta: {H.Format(v)}");
    return true;
}
// String eşleşme
if (H.Like(Event.Address, "MAIN_Simu.b*Sensor"))
{
    H.Reason($"Sensör tagi: {Event.Address}");
    return true;
}
// Zaman örneği
if (H.SecondsSince(Event.TsUtc) > 2.0)
{
    H.Reason($"Event eski: {H.Ago(Event.TsUtc)}");
    return true;
}
// Sayısal fark
var diff = H.Diff(H.Double("250"), v);
if (diff.HasValue && diff < -10)
{
    H.Reason($"Negatif fark: {diff:F1}");
    return true;
}
