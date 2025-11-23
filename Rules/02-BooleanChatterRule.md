# 🔄 Boolean Chatter Rule Documentation

`BooleanChatterRule` is a rule for monitoring fast toggling of boolean values (ON/OFF), also known as chatter. It checks for **maximum toggles per minute** and **minimum ON/OFF dwell time**.

---

## Key Properties

1. **_minOnMs** / **_minOffMs**  
   - Minimum time (in milliseconds) the value must remain ON or OFF before toggling is allowed.

2. **_maxTogglesPerMin**  
   - Maximum number of allowed toggles (ON → OFF → ON) per minute.

3. **_include and _exclude**  
   - Specify which addresses the rule should apply to.
   - Supports wildcard patterns (`*` and `?`).

4. **_map**  
   - Tracks the last state, last change time, and recent toggle times for each address.

---

## How It Works

1. The rule observes boolean events (`PlcEvent`) from a data source.  
2. If the event’s address matches the included patterns and is not excluded, the value is parsed as boolean.  
3. If the value changes (toggle detected), the rule checks:
   - If the **dwell time** (time since last state change) is shorter than the minimum ON/OFF duration.  
   - If the **toggle count in the last minute** exceeds the maximum allowed.
4. If either condition is violated, a **RuleHit** is generated with the reason.

---

## Examples

### 1. Basic Usage

```csharp
var rule = new BooleanChatterRule(minOnMs: 100, minOffMs: 100, maxTogglesPerMin: 60);

rule.Observe(new PlcEvent { Address = "Switch1", ValueText = "1", TsUtc = DateTime.UtcNow });
// First ON → no violation

// Immediately toggle OFF
rule.Observe(new PlcEvent { Address = "Switch1", ValueText = "0", TsUtc = DateTime.UtcNow.AddMilliseconds(50) });
// Violation: dwell too short (less than 100ms)
```

**Behavior:** Dwell time is enforced for ON/OFF states.

---

### 2. Max Toggles Per Minute

```csharp
var rule = new BooleanChatterRule(maxTogglesPerMin: 5);

// Toggle 6 times within a minute
for (int i = 0; i < 6; i++)
{
    rule.Observe(new PlcEvent { Address = "Switch2", ValueText = (i % 2 == 0 ? "1" : "0"), TsUtc = DateTime.UtcNow.AddSeconds(i * 10) });
}

// 6th toggle → violation: toggles/min > max
```

**Behavior:** Excessive toggling within a 1-minute window triggers a violation.

---

### 3. Include / Exclude Addresses

```csharp
var rule = new BooleanChatterRule(minOnMs: 100, minOffMs: 100, maxTogglesPerMin: 60,
    includeGlobs: new[] { "Sensor*" },
    excludeGlobs: new[] { "SensorTemp*" });

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "1", TsUtc = DateTime.UtcNow });
// Observed because it matches include pattern

rule.Observe(new PlcEvent { Address = "SensorTemp1", ValueText = "1", TsUtc = DateTime.UtcNow });
// Ignored because it matches exclude pattern
```

**Behavior:** Rules are applied only to selected addresses using wildcards.

### 4. With Custom Script

```csharp
// BooleanChatter (min ON/OFF + toggle sayısı)
// Parametreler
int minOnMs = 50, minOffMs = 50;
int maxTogglesPerMin = 60;
static readonly System.Collections.Generic.Dictionary<string,(bool has,bool prev,DateTime lastFlip,int toggles)> s
 = new(System.StringComparer.OrdinalIgnoreCase);
if (!H.Bool(Event.ValueText).HasValue) return false;
bool cur = H.Bool(Event.ValueText)!.Value;
var now = DateTime.UtcNow;
if (!s.TryGetValue(Event.Address, out var st)) { s[Event.Address]=(true,cur,now,0); return false; }
if (cur != st.prev) {
    var dt = (now - st.lastFlip).TotalMilliseconds;
    if ((cur && dt < minOffMs) || (!cur && dt < minOnMs))
        return H.Hit($"Chatter: flip too fast ({dt:0} ms)");
    // dakika penceresi için kaba sayım
    if (dt <= 60_000) st.toggles++; else st.toggles = 1;
    if (st.toggles > maxTogglesPerMin)
        return H.Hit($"Chatter: toggles>{maxTogglesPerMin}/min");
    s[Event.Address]=(true,cur,now,st.toggles);
    return false;
}
return false;
```
---

**Summary:**  
`BooleanChatterRule` monitors fast toggling of boolean signals, enforcing minimum dwell times for ON/OFF states and maximum toggle counts per minute, with optional filtering by address patterns.
