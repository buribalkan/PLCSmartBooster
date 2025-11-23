# ↕️ Analog Range Rule Documentation

`AnalogRangeRule` is a rule for monitoring analog (numeric) values to check whether they fall within a specified range. It can be configured to treat values inside or outside the range as a violation and can optionally use the absolute value of the measurement.

---

## Key Properties

1. **_low and _high**  
   - Define the numeric range `[low, high]`.  
   - If either is `NaN`, that boundary is ignored.

2. **_insideIsBad** (boolean)  
   - `false` → Values **outside the range** are violations (classic behavior).  
   - `true` → Values **inside the range** are violations.

3. **_useAbs** (boolean)  
   - `true` → Checks are applied to the **absolute value** of the number.  
   - `false` → Checks use the original value.

4. **_minDurationMs**  
   - Minimum duration (in milliseconds) the violation must persist before it is considered a hit.

5. **_include and _exclude**  
   - Specify which addresses the rule should apply to.  
   - Supports wildcard patterns (`*` and `?`).

---

## How It Works

1. The rule observes numeric events (`PlcEvent`) from a data source.  
2. If the event’s address matches the included patterns and is not excluded, the value is checked against the defined range.  
3. Depending on `_insideIsBad`:  
   - If `false`: the violation occurs **outside the range**.  
   - If `true`: the violation occurs **inside the range**.  
4. If `_useAbs` is true, the value is converted to its absolute value before checking.  
5. Violations are tracked over time, and only reported if they persist longer than `_minDurationMs`.

---

## Examples

### 1. Classic Range Check (outside is bad)

```csharp
var rule = new AnalogRangeRule(low: 10, high: 20); // default: insideIsBad=false, useAbs=false

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "5", TsUtc = DateTime.UtcNow });
// Violation: value 5 is below 10 → reported

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "15", TsUtc = DateTime.UtcNow });
// OK: value 15 is inside [10, 20] → no violation
```

**Behavior:** Only values outside `[10, 20]` are considered violations.

---

### 2. Inside is Bad

```csharp
var rule = new AnalogRangeRule(low: 10, high: 20, insideIsBad: true);

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "15", TsUtc = DateTime.UtcNow });
// Violation: value 15 is inside [10, 20] → reported

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "25", TsUtc = DateTime.UtcNow });
// OK: value 25 is outside [10, 20] → no violation
```

**Behavior:** Being inside the range triggers a violation.

---

### 3. Using Absolute Values

```csharp
var rule = new AnalogRangeRule(low: 0, high: 10, useAbs: true);

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "-8", TsUtc = DateTime.UtcNow });
// OK: abs(-8) = 8, which is inside [0, 10] → no violation

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "-15", TsUtc = DateTime.UtcNow });
// Violation: abs(-15) = 15, which is above 10 → reported
```

**Behavior:** Violations are calculated using the absolute value of the measurement.

---

### 4. Minimum Duration

```csharp
var rule = new AnalogRangeRule(low: 10, high: 20, minDurationMs: 5000);

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "5", TsUtc = DateTime.UtcNow });
// Not reported yet: duration < 5000ms

// Wait 5 seconds, then observe again
rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "5", TsUtc = DateTime.UtcNow.AddSeconds(5) });
// Violation reported: persisted longer than 5000ms
```

**Behavior:** Short-lived violations are ignored unless they persist beyond the specified duration.


### 5. With Custom Script

```csharp
//AnalogRange Exemple
// Ayarlar:
double low = 0, high = 100;
bool insideIsBad = false; // true ise [low,high] aralığında olmak ihlaldir
bool useAbs = false;      // true ise |v| kullan
var v = H.Double(Event.ValueText);
if (!v.HasValue) return false;
var x = useAbs ? Math.Abs(v.Value) : v.Value;
bool inside = (!double.IsNaN(low) && !double.IsNaN(high)) ? (x >= low && x <= high)
            : (!double.IsNaN(low)) ? (x >= low)
            : (!double.IsNaN(high)) ? (x <= high)
            : false;
bool viol = insideIsBad ? inside : !inside;
if (!double.IsNaN(low) || !double.IsNaN(high))
    return viol ? H.Hit($"AnalogRange: {x:G6} (low={low:G6}, high={high:G6})") : false;
return false;
```

---

**Summary:**  
`AnalogRangeRule` monitors numeric values, checks if they are inside or outside a range (configurable), can use absolute values, and can filter by duration and address.
