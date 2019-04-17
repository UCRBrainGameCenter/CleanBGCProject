using System;
using System.Collections;
using System.Collections.Generic;
using BGC.Audio;
using BGC.Mathematics;

public enum NoteProgression
{
    Chromatic = 0,
    Scale,
    Arpeggio,
    MAX
}

public class HarmonicComplex : HarmonicBase
{
    private const double Range = 120.0;

    private readonly double Duration;

    private readonly double fundamentalFreq;
    private readonly double angle;

    public HarmonicComplex(
        double lateralization,
        double tone,
        NoteProgression progression = NoteProgression.Scale,
        int octaveOffset = 0,
        int octaveSpan = 3)
    {
        IReadOnlyList<FrequencySet> scale = GetSequence(progression);

        int notes = scale.Count * octaveSpan + 1;
        int note = (int)Math.Floor((notes) * tone);
        note = GeneralMath.Clamp(note, 0, notes - 1);

        int octaves = octaveOffset + note / scale.Count;
        int noteIndex = note % scale.Count;

        fundamentalFreq = BaseFreqLB * Math.Pow(2, octaves) * scale[noteIndex].freqRatio;

        Duration = 0.25;

        //Angle is between -Range/2 and +Range/2
        angle = Range * (lateralization - 0.5);

        BuildStream();
    }


    public HarmonicComplex(
        int toneNum,
        NoteProgression progression = NoteProgression.Scale,
        int octaveOffset = 0,
        int octaveSpan = 3,
        double duration = 0.2)
    {
        IReadOnlyList<FrequencySet> scale = GetSequence(progression);

        int notes = scale.Count * octaveSpan + 1;
        int note = GeneralMath.Clamp(toneNum, 0, notes - 1);

        int octaves = octaveOffset + note / scale.Count;
        int noteIndex = note % scale.Count;

        fundamentalFreq = BaseFreqLB * Math.Pow(2, octaves) * scale[noteIndex].freqRatio;

        Duration = duration;

        angle = 0.0;

        BuildStream();
    }

    private void BuildStream()
    {
        stream = GetNote(fundamentalFreq, Duration)
            .Spatialize(angle)
            .Normalize(Level);
    }
}
