
# H.Signal — SignalHelper Fonksiyon Referansı

## Boolean Durum Fonksiyonları

### `bool StuckBool(string tag, int n, bool? stuckValue = null)`
Boolean stuck detection.

### `bool RisingEdgeBool(string tag)`
False→True edge.

### `bool FallingEdgeBool(string tag)`
True→False edge.

### `bool DebounceBool(string tag, int requiredStableCount, bool requireTrue = true)`
Debounce detection.

### `bool Chatter(string tag, int n, int maxToggles)`
Chatter detection.

## Analog Fonksiyonlar

### `bool Spike(string tag, int n, double absThreshold, double pctThreshold)`
Spike detection.

### `bool? HysteresisAnalog(string tag, int n, double low, double high)`
Analog hysteresis.

### `bool? HysteresisBool(string tag, int k, bool requireTrue = true)`
Boolean hysteresis.

## Zaman Tabanlı Fonksiyonlar

### `bool Deadman(string tag, int n, double sampleIntervalSec, double maxSilentSec)`
Deadman detection.

### `string? TrendDirection(string tag, int n, double threshold = 0.0)`
Trend direction.
