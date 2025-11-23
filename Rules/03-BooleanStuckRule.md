# 🔒 Boolean Stuck Rule Documentation

`BooleanStuckRule` is a rule that monitors boolean tags and generates a hit if a tag remains in the same state (true/false) for too long. It works on a per-address basis and can optionally respect include/exclude address patterns.

---

## Key Properties

1. **_stuckMs**  
   - The minimum time (in milliseconds) a boolean value must remain unchanged to be considered "stuck".

2. **_minMsBetweenHits**  
   - Minimum time between consecutive hits for the same address to prevent flooding.

3. **_include and _exclude**  
   - Specify which addresses the rule should monitor.
   - Supports wildcard patterns (`*` and `?`).

4. **_map**  
   - Tracks the last value, last flip time, and last hit time for each address.

5. **_pending**  
   - Stores a pending `RuleHit` that will be returned on `TryHit()`.

---

## How It Works

1. The rule observes boolean events (`PlcEvent`).  
2. For each event:
   - If the address matches the include/exclude patterns and the value can be parsed as boolean, it updates the tracked state.
   - If the value has changed since the last observation, the "stuck" timer resets.
   - If the value has not changed for `_stuckMs` milliseconds and `_minMsBetweenHits` has passed, a hit is generated.
3. Additionally, on every observation, the rule evaluates all addresses in the background in case a tag is "stuck" even without a new event.

---

## Examples

### 1. Basic Usage

```csharp
var rule = new BooleanStuckRule(name: "StuckMonitor", stuckMs: 5000, minMsBetweenHits: 1000);

// Initial observation
rule.Observe(new PlcEvent { Address = "Switch1", ValueText = "1", TsUtc = DateTime.UtcNow });

// After 5 seconds, without changing the value
rule.Observe(new PlcEvent { Address = "Switch1", ValueText = "1", TsUtc = DateTime.UtcNow.AddSeconds(5) });

var hit = rule.TryHit();
// Hit is generated: Switch1 has been stuck ON for 5000 ms
```

**Behavior:** A hit is generated if a boolean value remains unchanged for longer than the threshold.

---

### 2. Minimum Time Between Hits

```csharp
var rule = new BooleanStuckRule(name: "StuckMonitor", stuckMs: 5000, minMsBetweenHits: 10000);

// Switch1 stays ON for 5 seconds → first hit
rule.Observe(new PlcEvent { Address = "Switch1", ValueText = "1", TsUtc = DateTime.UtcNow });
rule.Observe(new PlcEvent { Address = "Switch1", ValueText = "1", TsUtc = DateTime.UtcNow.AddSeconds(5) });
var hit1 = rule.TryHit();

// Even if value remains ON for another 5 seconds, no new hit due to minMsBetweenHits
rule.Observe(new PlcEvent { Address = "Switch1", ValueText = "1", TsUtc = DateTime.UtcNow.AddSeconds(10) });
var hit2 = rule.TryHit();
// hit2 is null
```

**Behavior:** Prevents repeated hits for the same address too frequently.

---

### 3. Include / Exclude Addresses

```csharp
var rule = new BooleanStuckRule(stuckMs: 5000, minMsBetweenHits: 1000,
    include: new[] { "Sensor*" },
    exclude: new[] { "SensorTemp*" });

rule.Observe(new PlcEvent { Address = "Sensor1", ValueText = "1", TsUtc = DateTime.UtcNow });
// Observed because it matches include pattern

rule.Observe(new PlcEvent { Address = "SensorTemp1", ValueText = "1", TsUtc = DateTime.UtcNow });
// Ignored because it matches exclude pattern
```

**Behavior:** Only monitors addresses selected via include/exclude patterns.

### 4. With Custom Script

```csharp
//BooleanStuck (aynı değerde ≥ süre)
int stuckMs = 60_000;
static readonly System.Collections.Generic.Dictionary<string,(bool has,bool prev,DateTime lastFlip)> s
    = new(System.StringComparer.OrdinalIgnoreCase);
var b = H.Bool(Event.ValueText);
if (!b.HasValue) return false;
bool cur = b.Value; var now = DateTime.UtcNow;
if (!s.TryGetValue(Event.Address, out var st)) { s[Event.Address]=(true,cur,now); return false; }
if (cur != st.prev) { s[Event.Address]=(true,cur,now); return false; }
var ms = (now - st.lastFlip).TotalMilliseconds;
return ms >= stuckMs ? H.Hit($"BooleanStuck: {cur} for {ms:0} ms") : false;
```



---

**Summary:**  
`BooleanStuckRule` detects boolean tags that remain in the same state for too long, enforces minimum time between hits, and allows address-based filtering with wildcards.
