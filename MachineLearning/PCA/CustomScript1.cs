// ====== PCA (ML.NET RandomizedPCA) CustomScript ======
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System.Globalization;
// --- Ayarlar ---
string[] FEATS = new[] {
    // → buraya feature tag’larını ekle
    // örn: "GVL.MotorCurrent", "GVL.Torque", "GVL.Temperature"
};
const int COOLDOWN_MS = 1000;
const double SCORE_THR = 0.50;   // anomali eşiği
if (FEATS.Length < 2) { H.Log?.Invoke("PCA: en az 2 feature gerekli."); return false; }
// I/O şeması
sealed class InRow { [VectorType] public float[]? Features { get; set; } }
sealed class OutRow { public float Score { get; set; } }
// ML tekil
static MLContext? _ml;
static ITransformer? _model;
static PredictionEngine<InRow, OutRow>? _engine;
// state
static readonly Dictionary<string,double> _last = new(StringComparer.OrdinalIgnoreCase);
static DateTime _lastHit = DateTime.MinValue;
void EnsureMl()
{
    if (_ml != null) return;
    _ml = new MLContext(seed: 1);
    // sabit şema (vektör uzunluğu = FEATS.Length)
    var bootstrap = Enumerable.Range(0, 64)
        .Select(_ => new InRow { Features = new float[FEATS.Length] })
        .ToList();
    var schema = SchemaDefinition.Create(typeof(InRow));
    schema[nameof(InRow.Features)].ColumnType =
        new VectorDataViewType(NumberDataViewType.Single, FEATS.Length);
    var data = _ml.Data.LoadFromEnumerable(bootstrap, schema);
    var pipe = _ml.AnomalyDetection.Trainers.RandomizedPca(
        featureColumnName: nameof(InRow.Features),
        rank: Math.Min(3, FEATS.Length),
        ensureZeroMean: true,
        seed: 1
    );
    _model = pipe.Fit(data);
    _engine = _ml.Model.CreatePredictionEngine<InRow, OutRow>(_model);
}
// değerleri besle (event ne geliyorsa cache’le)
if (!string.IsNullOrEmpty(Event.Address) &&
    double.TryParse(Event.ValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
{
    // sadece FEATS listesindeki tag’ları önemsiyoruz:
    if (FEATS.Contains(Event.Address, StringComparer.OrdinalIgnoreCase))
        _last[Event.Address] = d;
}
EnsureMl();
// tüm feature’ların son değeri yoksa bekle
if (FEATS.Any(f => !_last.ContainsKey(f))) return false;
// cooldown
var now = Event.TsUtc == default ? DateTime.UtcNow : Event.TsUtc;
if (COOLDOWN_MS > 0 && (now - _lastHit).TotalMilliseconds < COOLDOWN_MS)
    return false;
// vektörü oluştur
var vec = FEATS.Select(f => (float)_last[f]).ToArray();
// skorla
var pred = _engine!.Predict(new InRow { Features = vec });
var score = pred.Score;
if (score >= SCORE_THR)
{
    _lastHit = now;
    var summary = string.Join(", ", FEATS.Select((f,i) => $"{f}={vec[i]:0.###}"));
    return H.Hit($"PCA anomaly: score={score:0.###} ≥ {SCORE_THR} | {summary}");
}
return false;
