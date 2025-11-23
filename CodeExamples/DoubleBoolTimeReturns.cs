//Double
return Event.Address.EndsWith(".Speed") && (ValueAsDouble() ?? 0) > 100;
//Boolean/String:
return Event.Address.Contains("Lamp") && IsTrue();
//Zaman kullanımı:
return (DateTime.UtcNow - Event.TsUtc).TotalSeconds < 5 && (ValueAsDouble() ?? 0) >= 50;
