// ====== SSA Spike (ML.NET) CustomScript ======
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TimeSeries;
using System.Globalization;
// --- Ayarlar ---
const int WINDOW = 64;                 // seasonality window
const int TRAIN = 5 * WINDOW;          // training window
const int PVHIST = 3 * WINDOW;         // p-value history
const int COOLDOWN_MS = 1000;          // tekrar baskılama
// I/O şeması
sealed class InRow { public float Value { get; set; } }
sealed class OutRow { [VectorType] public double[]? Prediction { get; set; } } // [Alert, RawScore, PValue]
// ML tekil
static MLContext? _ml;
static ITransformer? _ssa;
// State
sealed class Series {
    public readonly Queue<float> Buf = new();
    public DateTime LastHitUtc = DateTime.MinValue;
}
static readonly Dictionary<string, Series> _per = new(StringComparer.OrdinalIgnoreCase);
void EnsureMl()
{
    if (_ml != null) return;
    _ml = new MLContext(seed: 1);
    var empty = _ml.Data.LoadFromEnumerable(Array.Empty<InRow>());
    var pipe = _ml.Transforms.DetectSpikeBySsa(
        outputColumnName: "Prediction",
        inputColumnName: nameof(InRow.Value),
        confidence: 95,
        pvalueHistoryLength: PVHIST,
        trainingWindowSize: TRAIN,
        seasonalityWindowSize: WINDOW
    );
    _ssa = pipe.Fit(empty);
}
EnsureMl();
string addr = Event.Address ?? "";
if (string.IsNullOrEmpty(addr)) return false;
if (!double.TryParse(Event.ValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
    return false;
var ser = _per.TryGetValue(addr, out var s) ? s : (_per[addr] = new Series());
ser.Buf.Enqueue((float)d);
int keep = Math.Max(TRAIN, PVHIST);
while (ser.Buf.Count > keep) ser.Buf.Dequeue();
if (ser.Buf.Count < keep) return false;
// cooldown
var now = Event.TsUtc == default ? DateTime.UtcNow : Event.TsUtc;
if (COOLDOWN_MS > 0 && (now - ser.LastHitUtc).TotalMilliseconds < COOLDOWN_MS)
    return false;
// skorla
var dv = _ml!.Data.LoadFromEnumerable(ser.Buf.Select(v => new InRow { Value = v }));
var scored = _ssa!.Transform(dv);
var last = _ml.Data.CreateEnumerable<OutRow>(scored, reuseRowObject: true).LastOrDefault();
var v = last?.Prediction; // [Alert, RawScore, PValue]
if (v is { Length: >= 1 } && v[0] > 0.5)
{
    ser.LastHitUtc = now;
    double raw = (v.Length > 1) ? v[1] : double.NaN;
    double pv = (v.Length > 2) ? v[2] : double.NaN;
    return H.Hit($"SSA spike: raw={raw:0.###}, p={pv:0.###}");
}
return false;
