// SpectralDoctor v3 — MathNet FFT + robust peak prominence + TopN peaks + band energies + CSV/PNG
// Requires: MathNet.Numerics (Fourier), H.* helpers available
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using MathNet.Numerics.IntegralTransforms;
// ---------------- CONFIG ----------------
string tag = "GVL_Var.fTempActual";
double sampleIntervalSec = 1;
int n = 512; // preferred power-of-two
double minPeakFreqHz = 0.01;
double maxPeakFreqHz = 5.0;
int topNPeaks = 5;            // kaç peak raporlayalım
int bands = 8;                // bandEnergy bölmeleri
double minEnergyForActivity = 1e-6;
string outDir = "SpectralDoctor";
string baseFile = "Spectrum_" + tag.Replace('.', '_');
// Optional: çalıştırma flag (tek seferlik demo). Kapatmak için false yap.
static bool CompletedV3 = false;
if (CompletedV3) return;
// ---------------- GET DATA ----------------
var xsFloat = H.ML.FeatureVectorFromLastN(tag, n, normalize: false);
if (xsFloat == null || xsFloat.Length < n)
{
    int have = xsFloat?.Length ?? 0;
    H.Console($"SpectralDoctorV3: not enough samples for {tag} (have={have}, need={n}).");
    return;
}
// convert to double array
double[] sig = xsFloat.Select(x => (double)x).ToArray();
// remove mean (DC)
double mean = sig.Average();
for (int i = 0; i < n; i++) sig[i] -= mean;
// Hann window to reduce spectral leakage
for (int i = 0; i < n; i++)
{
    double w = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (n - 1)));
    sig[i] *= w;
}
// ---------------- FFT via MathNet (in-place) ----------------
var complex = new Complex[n];
for (int i = 0; i < n; i++) complex[i] = new Complex(sig[i], 0.0);
// Forward FFT (Matlab scaling, consistent with earlier scripts)
Fourier.Forward(complex, FourierOptions.Matlab);
int m = n / 2; // use 0..N/2
double fs = 1.0 / sampleIntervalSec;
double[] freqs = new double[m];
double[] mags  = new double[m];
for (int k = 0; k < m; k++)
{
    double re = complex[k].Real;
    double im = complex[k].Imaginary;
    double mag = Math.Sqrt(re * re + im * im) / n; // normalize by n
    mags[k] = mag;
    freqs[k] = k * fs / n;
}
// energies
double EPS = 1e-18;
double[] energies = mags.Select(v => v * v).ToArray();
double totalEnergy = energies.Sum();
bool noActivity = totalEnergy < minEnergyForActivity;
// band energies (normalized to totalEnergy)
int[] bandStart = new int[bands];
int[] bandEnd = new int[bands];
double[] bandEnergy = new double[bands];
for (int b = 0; b < bands; b++)
{
    int s = (int)Math.Round(b * (double)m / bands);
    int e = (int)Math.Round((b + 1) * (double)m / bands);
    if (s < 0) s = 0;
    if (e > m) e = m;
    bandStart[b] = s; bandEnd[b] = e;
    bandEnergy[b] = energies.Skip(s).Take(e - s).Sum();
}
if (totalEnergy > 0)
    for (int b = 0; b < bands; b++) bandEnergy[b] /= totalEnergy;
// ---------------- Peak detection (local maxima) ----------------
List<int> candidateIdx = new List<int>();
for (int k = 1; k < m - 1; k++)
{
    if (mags[k] > mags[k - 1] && mags[k] >= mags[k + 1])
    {
        // apply freq band limits
        double f = freqs[k];
        if (f >= minPeakFreqHz && (maxPeakFreqHz <= 0 || f <= maxPeakFreqHz))
            candidateIdx.Add(k);
    }
}
// Helper: find nearest index to left that has magnitude > mags[peak], otherwise -1 (edge)
int FindFirstHigherToLeft(int peak)
{
    for (int i = peak - 1; i >= 0; i--)
        if (mags[i] > mags[peak]) return i;
    return -1;
}
// Helper: find nearest index to right that has magnitude > mags[peak], otherwise -1
int FindFirstHigherToRight(int peak)
{
    for (int i = peak + 1; i < m; i++)
        if (mags[i] > mags[peak]) return i;
    return -1;
}
// Helper: find minimum in interval (exclusive ends)
double MinInRange(int aInclusive, int bExclusive)
{
    if (bExclusive <= aInclusive) return double.MaxValue;
    double mn = double.MaxValue;
    for (int i = aInclusive; i < bExclusive && i < m; i++)
        if (mags[i] < mn) mn = mags[i];
    return (mn == double.MaxValue) ? 0.0 : mn;
}
// Compute prominence + width for each candidate
var peaks = new List<(int idx, double freq, double mag, double power, double prominence, double widthHz)>();
foreach (var k in candidateIdx)
{
    double mag = mags[k];
    double power = energies[k];
    // left/right higher peaks
    int leftHigher = FindFirstHigherToLeft(k);
    int rightHigher = FindFirstHigherToRight(k);
    // left baseline minimum
    double leftMin;
    if (leftHigher >= 0)
    {
        leftMin = MinInRange(leftHigher + 1, k); // min between leftHigher and peak
    }
    else
    {
        leftMin = MinInRange(0, k);
    }
    // right baseline minimum
    double rightMin;
    if (rightHigher >= 0)
    {
        rightMin = MinInRange(k + 1, rightHigher); // min between peak and rightHigher
    }
    else
    {
        rightMin = MinInRange(k + 1, m);
    }
    // baseline is max(leftMin, rightMin) per standard prominence def
    double baseline = Math.Max(leftMin, rightMin);
    double prominence = Math.Max(0.0, mag - baseline);
    // width: half-prominence (half-height above baseline)
    double halfHeight = baseline + prominence / 2.0;
    // find left crossing index where mags <= halfHeight
    int li = k;
    while (li > 0 && mags[li] > halfHeight) li--;
    int ri = k;
    while (ri < m - 1 && mags[ri] > halfHeight) ri++;
    // linear interp for left/right freq (simple)
    double fLeft = freqs[Math.Max(0, li)];
    double fRight = freqs[Math.Min(m - 1, ri)];
    double widthHz = Math.Max(0.0, fRight - fLeft);
    peaks.Add((k, freqs[k], mag, power, prominence, widthHz));
}
// Keep only peaks with non-zero prominence and sort by prominence desc (or power)
var significantPeaks = peaks.Where(p => p.prominence > 0).OrderByDescending(p => p.prominence).ToList();
// If none significant, fallback to highest mag
if (significantPeaks.Count == 0 && peaks.Count > 0)
{
    var best = peaks.OrderByDescending(p => p.mag).First();
    significantPeaks.Add(best);
}
// top N
var topPeaks = significantPeaks.Take(topNPeaks).ToArray();
// oscillator index
double peakPowerTotal = topPeaks.Length > 0 ? topPeaks[0].power : 0.0;
double oscIndex = (totalEnergy > 0.0) ? peakPowerTotal / totalEnergy : 0.0;
// ---------------- Health scoring (same logic with penalties)
double maxOscIndexGood = 0.30;
double maxHighFreqFracGood = 0.25;
double health = 100.0;
if (oscIndex > maxOscIndexGood)
{
    double over = oscIndex - maxOscIndexGood;
    health -= 40.0 * Math.Min(over / 0.5, 1.0);
}
if (bandEnergy.Skip(bands * 7 / 8).Sum() > maxHighFreqFracGood) { /* not used; we computed highFreqFrac earlier in v2 style */ }
double highFreqFrac = bandEnergy.Skip((int)(bands * 0.7)).Sum();
if (highFreqFrac > maxHighFreqFracGood)
{
    double over = highFreqFrac - maxHighFreqFracGood;
    health -= 30.0 * Math.Min(over / 0.5, 1.0);
}
if (noActivity) health -= 30.0;
health = Math.Max(0.0, Math.Min(100.0, health));
string grade = health >= 90 ? "A" : health >= 75 ? "B" : health >= 60 ? "C" : health >= 40 ? "D" : "E";
//// ---------------- Save PNGs (linear + log)
//try
//{
//    H.Plot.SaveLinePng(outDir, baseFile + "_linear", freqs, mags, title: $"Spectrum {tag}", xLabel: "Hz", yLabel: "Amplitude");
//}
//catch (Exception ex) { H.ConsoleError("PNG linear save error: " + ex.Message); }
//double[] magsLog = mags.Select(v => Math.Log10(v + 1e-12)).ToArray();
//try
//{
//    H.Plot.SaveLinePng(outDir, baseFile + "_log", freqs, magsLog, title: $"LogSpectrum {tag}", xLabel: "Hz", yLabel: "log10(Amplitude)");
//}
//catch (Exception ex) { H.ConsoleError("PNG log save error: " + ex.Message); }
string logSpecFile = $"{outDir}/{baseFile}_log.png";
// ---------------- CSV / LogSample (include top peaks and band energies)
var cols = new List<(string, object?)>
{
    ("Tag", tag),
    ("N", n),
    ("SampleIntervalSec", sampleIntervalSec),
    ("TotalEnergy", totalEnergy),
    ("OscillationIndex", oscIndex),
    ("HealthIndex", health),
    ("Grade", grade),
    ("LogSpectrumFile", logSpecFile)
};
for (int b = 0; b < bands; b++) cols.Add( ($"BandEnergy{b}", bandEnergy[b]) );
// Top peaks summary columns
for (int i = 0; i < topNPeaks; i++)
{
    if (i < topPeaks.Length)
    {
        var p = topPeaks[i];
        cols.Add( ($"Peak{i}_FreqHz", p.freq) );
        cols.Add( ($"Peak{i}_Mag", p.mag) );
        cols.Add( ($"Peak{i}_Power", p.power) );
        cols.Add( ($"Peak{i}_Prom", p.prominence) );
        cols.Add( ($"Peak{i}_WidthHz", p.widthHz) );
    }
    else
    {
        cols.Add( ($"Peak{i}_FreqHz", 0.0) );
        cols.Add( ($"Peak{i}_Mag", 0.0) );
        cols.Add( ($"Peak{i}_Power", 0.0) );
        cols.Add( ($"Peak{i}_Prom", 0.0) );
        cols.Add( ($"Peak{i}_WidthHz", 0.0) );
    }
}
H.LogSample(outDir, "SpectralDoctorV3_Summary_" + tag.Replace('.', '_'), cols.ToArray());
// ---------------- Console summary
H.Console("===========================================");
H.Console($"SpectralDoctorV3 Tag={tag} N={n} dt={sampleIntervalSec:F3}s Fs={fs:F2}Hz");
H.Console($"TotalEnergy={totalEnergy:E3} HighFreqFrac={highFreqFrac:P2} OscIndex={oscIndex:P2} Health={health:F1} Grade={grade}");
if (topPeaks.Length > 0)
{
    for (int i = 0; i < topPeaks.Length; i++)
    {
        var p = topPeaks[i];
        H.Console($"Peak#{i}: f={p.freq:F4} Hz mag={p.mag:E4} prom={p.prominence:E4} width={p.widthHz:F4} Hz power={p.power:E4}");
    }
}
else
{
    H.Console("No significant peaks found in range.");
}
H.Console($"Band energies: {string.Join(", ", bandEnergy.Select(b => b.ToString("F4")))}");
H.Console("===========================================");
// mark completed if you want single-run behavior
CompletedV3 = true;
return;
