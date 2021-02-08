using System.Collections.Generic;
using BGC.Mathematics;
using BGC.Audio;
using BGC.Audio.Filters;
using BGC.Audio.Synthesis;

using static System.Math;

public abstract class HarmonicBase : IBGCStream
{
    protected double SamplingRate => 44100.0;

    protected const double BaseFreqLB = 65.406;
    protected const double Level = 80.0;

    protected IBGCStream stream = null;

    protected readonly struct FrequencySet
    {
        public readonly double freqRatio;
        public readonly double amplitude;

        public FrequencySet(double freqRatio, double amplitude)
        {
            this.freqRatio = freqRatio;
            this.amplitude = amplitude;
        }

        public ComplexCarrierTone GetCarrier(double fundamentalFreq) =>
            new ComplexCarrierTone(
                frequency: fundamentalFreq * freqRatio,
                amplitude: Complex64.FromPolarCoordinates(
                    magnitude: amplitude,
                    phase: 2.0 * PI * CustomRandom.NextDouble()));
    };

    protected static readonly IReadOnlyList<FrequencySet> PositiveChord = new List<FrequencySet>()
    {
        new FrequencySet(1.0, 1.5),
        new FrequencySet(5.0 / 4.0, 1.0),
        new FrequencySet(3.0 / 2.0, 1.0),
        new FrequencySet(2.0, 1.0)
    };

    protected static readonly IReadOnlyList<FrequencySet> NegativeChord = new List<FrequencySet>()
    {
        new FrequencySet(1.0, 1.5),
        new FrequencySet(12.0 / 10.0, 1.0),
        new FrequencySet(15.0 / 10.0, 1.0),
        new FrequencySet(15.0 / 8.0, 1.0)
    };

    protected static readonly IReadOnlyList<FrequencySet> Scale = new List<FrequencySet>()
    {
        new FrequencySet(1.0, 1.0),
        new FrequencySet(9.0 / 8.0, 1.0),
        new FrequencySet(5.0 / 4.0, 1.0),
        new FrequencySet(4.0 / 3.0, 1.0),
        new FrequencySet(3.0 / 2.0, 1.0),
        new FrequencySet(5.0 / 3.0, 1.0),
        new FrequencySet(15.0 / 8.0, 1.0)
    };

    protected static readonly IReadOnlyList<FrequencySet> Chromatic = new List<FrequencySet>()
    {
        new FrequencySet(1.0, 1.0),
        new FrequencySet(Pow(2, 1.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 2.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 3.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 4.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 5.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 6.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 7.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 8.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 9.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 10.0 / 12.0), 1.0),
        new FrequencySet(Pow(2, 11.0 / 12.0), 1.0)
    };

    protected static readonly IReadOnlyList<FrequencySet> Arpeggio = new List<FrequencySet>()
    {
        new FrequencySet(1.0, 1.0),
        new FrequencySet(5.0 / 4.0, 1.0),
        new FrequencySet(3.0 / 2.0, 1.0)
    };

    protected static IReadOnlyList<FrequencySet> GetSequence(NoteProgression progression)
    {
        switch (progression)
        {
            case NoteProgression.Chromatic: return Chromatic;
            case NoteProgression.Scale: return Scale;
            case NoteProgression.Arpeggio: return Arpeggio;

            default:
                UnityEngine.Debug.LogError($"Unexpected NoteProgression: {progression}");
                return Scale;
        }
    }

    protected static IBGCStream GetNote(double frequency, double duration)
    {
        double rise = 0.0125;
        double sus = 0.15;
        duration -= rise + sus;

        return new StreamAdder(
            new TriangleWave(0.1, 0.5 * frequency),
            new TriangleWave(1.0, frequency),
            new TriangleWave(1.0, 1.5 * frequency),
            new TriangleWave(1.0, 2 * frequency),
            new TriangleWave(1.0, 4 * frequency),
            new TriangleWave(1.0, 8 * frequency))
            .StandardizeRMS()
            .ADSR(rise, sus, duration, 0.8, 100, 0.0125);
    }

    #region IBGCStream


    int IBGCStream.Channels => stream.Channels;
    int IBGCStream.TotalSamples => stream.TotalSamples;
    int IBGCStream.ChannelSamples => stream.ChannelSamples;
    float IBGCStream.SamplingRate => stream.SamplingRate;

    void IBGCStream.Initialize() => stream.Initialize();
    int IBGCStream.Read(float[] data, int offset, int count) => stream.Read(data, offset, count);
    void IBGCStream.Reset() => stream.Reset();
    void IBGCStream.Seek(int position) => stream.Seek(position);
    IEnumerable<double> IBGCStream.GetChannelRMS() => stream.GetChannelRMS();

    //No resources to dispose
    public void Dispose() { }

    #endregion IBGCStream
}
