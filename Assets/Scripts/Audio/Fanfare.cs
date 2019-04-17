using System;
using BGC.Audio;
using BGC.Audio.Filters;

public class Fanfare : HarmonicBase
{
    public static Fanfare MedalFanfare()
    {
        double fundamentalFreq = 4.0 * BaseFreqLB;

        double[] frequencies = new double[]
        {
            Arpeggio[0].freqRatio * fundamentalFreq,
            Arpeggio[1].freqRatio * fundamentalFreq,
            Arpeggio[2].freqRatio * fundamentalFreq,
            2.0 * fundamentalFreq
        };

        return new Fanfare(
            noteDuration: 0.1,
            endSustain: 0.5,
            frequencies: frequencies);
    }

    public static Fanfare LevelCompleteFanfare()
    {
        double fundamentalFreq = 4.0 * BaseFreqLB;

        double[] frequencies = new double[]
        {
            Arpeggio[0].freqRatio * fundamentalFreq,
            Arpeggio[1].freqRatio * fundamentalFreq,
            Arpeggio[2].freqRatio * fundamentalFreq,
            2.0 * fundamentalFreq
        };

        return new Fanfare(
            noteDuration: 0.125,
            endSustain: 0.5,
            frequencies: frequencies);
    }

    public static Fanfare MaxScoreFanfare()
    {
        double fundamentalFreq = 4.0 * BaseFreqLB;

        double[] frequencies = new double[]
        {
            Arpeggio[0].freqRatio * fundamentalFreq,
            Arpeggio[2].freqRatio * fundamentalFreq,
            2.0 * fundamentalFreq
        };

        return new Fanfare(
            noteDuration: 0.1,
            endSustain: 0.3,
            frequencies: frequencies);
    }

    public static Fanfare StaticSpawnFanfare()
    {
        double fundamentalFreq = BaseFreqLB;

        double[] frequencies = new double[]
        {
            Arpeggio[0].freqRatio * fundamentalFreq,
            Arpeggio[1].freqRatio * fundamentalFreq,
            Arpeggio[2].freqRatio * fundamentalFreq,
            2.0 * Arpeggio[0].freqRatio * fundamentalFreq,
            2.0 * Arpeggio[1].freqRatio * fundamentalFreq,
            2.0 * Arpeggio[2].freqRatio * fundamentalFreq,
            4.0 * Arpeggio[0].freqRatio * fundamentalFreq
        };

        return new Fanfare(
            noteDuration: 0.05,
            endSustain: 0.3,
            frequencies: frequencies);
    }

    private Fanfare(
        double noteDuration,
        double endSustain,
        double[] frequencies)
    {
        double totalDuration = noteDuration * 4 + endSustain;
        int totalSamples = (int)Math.Floor(SamplingRate * totalDuration);

        StreamAdder fanfareStream = new StreamAdder();

        for (int i = 0; i < frequencies.Length; i++)
        {
            double startingTime = i * noteDuration;

            IBGCStream noteStream =
                GetNote(frequencies[i], totalDuration - startingTime)
                .Center((int)(startingTime * SamplingRate), 0);

            fanfareStream.AddStream(noteStream);
        }

        stream = fanfareStream
            .Spatialize(0.0)
            .Normalize(Level);
    }
}
