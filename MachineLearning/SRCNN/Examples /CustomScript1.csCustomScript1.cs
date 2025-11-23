// ====== SR-CNN (ML.NET) CustomScript ======
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TimeSeries;
using System.Globalization;
// --- Ayarlar ---
const int WINDOW = 128;         // pencere uzunluğu
const int LOOKAHEAD = 5;        // ileri bakış
const double THRESH = 0.30;     // SR-CNN eşik (0..1 arası)
const int COOLDOWN_MS = 2000;   // tekrar baskılama
// --- SR-CNN I/O şeması ---
sealed class InRow { public float Value { get; set; } }
sealed class OutRow { [VectorType] public double[]? Prediction { get; set; } }
// --- Per-address state ---
sealed class Series {
    public readonly Queue<float> Buf = new();
    public DateTime LastHitUtc = DateTime.MinValue;
    public float LastVal = float.NaN;
    public bool LastWasAnomaly = false;
}
// Tek MLContext/Transformer (paylaşımlı)
static MLContext? _ml;
static ITransformer? _sr;
// Adres -> Seri sözlüğü
static readonly Dictionary<string, Series> _per = new(StringComparer.OrdinalIgnoreCase);
// ML hazırlığı (bir defa)
void EnsureMl()
{
    if (_ml != null) return;
    _ml = new MLContext(seed: 1);
    var empty = _ml.Data.LoadFromEnumerable(Array.Empty<InRow>());
    var pipe = _ml.Transforms.DetectAnomalyBySrCnn(
        outputColumnName: "Prediction",
        inputColumnName: nameof(InRow.Value),
        windowSize: WINDOW,
        threshold: THRESH,
        lookaheadWindowSize: LOOKAHEAD
    );
    _sr = pipe.Fit(empty);
}
EnsureMl();
// Bu script tek bir adres üzerinde çalışacaksa Event’i kullan
string addr = Event.Address ?? "";
if (string.IsNullOrEmpty(addr)) return false;
// Değeri al
if (!double.TryParse(Event.ValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
    return false;
var ser = _per.TryGetValue(addr, out var s) ? s : (_per[addr] = new Series());
// buffer yönetimi
ser.Buf.Enqueue((float)d);
while (ser.Buf.Count > WINDOW) ser.Buf.Dequeue();
if (ser.Buf.Count < WINDOW) return false;
// cooldown
var now = Event.TsUtc == default ? DateTime.UtcNow : Event.TsUtc;
if (COOLDOWN_MS > 0 && (now - ser.LastHitUtc).TotalMilliseconds < COOLDOWN_MS)
    return false;
// skorla
var dv = _ml!.Data.LoadFromEnumerable(ser.Buf.Select(v => new InRow { Value = v }));
var scored = _sr!.Transform(dv);
var last = _ml.Data.CreateEnumerable<OutRow>(scored, reuseRowObject: true).LastOrDefault();
var vec = last?.Prediction;
if (vec == null || vec.Length == 0) return false;
// pratikte son eleman skordur
double score = vec[^1];
bool isAnomaly = score >= THRESH;
// edge trigger: normal->anomali
if (isAnomaly && !ser.LastWasAnomaly)
{
    float cur = ser.Buf.Last();
    string reason = $"SR-CNN anomaly: score={score:0.###} ≥ {THRESH:0.###}";
    if (!float.IsNaN(ser.LastVal))
    {
        float delta = cur - ser.LastVal;
        reason += $" | change {ser.LastVal:0.###} → {cur:0.###} (Δ={delta:0.###})";
        if (Math.Abs(delta) > 180 && cur * ser.LastVal < 0) reason += " | possible wrap-around";
    }
    ser.LastHitUtc = now;
    ser.LastWasAnomaly = true;
    ser.LastVal = cur;
    return H.Hit(reason);
}
// state güncelle
ser.LastWasAnomaly = isAnomaly;
ser.LastVal = ser.Buf.Last();
return false;
