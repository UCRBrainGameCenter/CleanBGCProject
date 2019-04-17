using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BGC.Audio;
using BGC.Audio.Filters;
using BGC.Mathematics;

public class FeedbackChords : HarmonicBase
{
    private const double Range = 120.0;

    private readonly double Duration;
    private readonly double fundamentalFreq;
    private readonly double angle;
    private readonly bool major;

    private IReadOnlyList<FrequencySet> Chord => major ? PositiveChord : NegativeChord;

    /// <summary>
    /// Collapses the tone to the nearest chromatic note
    /// </summary>
    public FeedbackChords(
        bool major,
        double lateralization,
        double tone,
        NoteProgression progression = NoteProgression.Scale,
        int octaveOffset = 0,
        int octaveSpan = 3)
    {
        this.major = major;

        IReadOnlyList<FrequencySet> scale = GetSequence(progression);

        int notes = scale.Count * octaveSpan + 1;
        int note = (int)Math.Floor(notes * tone);
        note = GeneralMath.Clamp(note, 0, notes - 1);

        int octaves = octaveOffset + note / scale.Count;
        int noteIndex = note % scale.Count;

        fundamentalFreq = BaseFreqLB * Math.Pow(2, octaves) * scale[noteIndex].freqRatio;

        Duration = major ? 0.2 : 0.4;

        //Angle is between -Range/2 and +Range/2
        angle = Range * (lateralization - 0.5);

        BuildStream();
    }

    /// <summary>
    /// Generates the "<paramref name="tone"/>"th note of a 3-octave C Scale
    /// </summary>
    public FeedbackChords(
        bool major,
        int tone,
        NoteProgression progression = NoteProgression.Scale,
        int octaveOffset = 0,
        int octaveSpan = 3)
    {
        this.major = major;

        IReadOnlyList<FrequencySet> scale = GetSequence(progression);

        //Note spans 3 octaves of CMajor scale.
        int notes = scale.Count * octaveSpan + 1;
        int note = GeneralMath.Clamp(tone, 0, notes - 1);

        int octaves = octaveOffset + note / scale.Count;
        int noteIndex = note % scale.Count;

        fundamentalFreq = BaseFreqLB * Math.Pow(2, octaves) * scale[noteIndex].freqRatio;

        Duration = major ? 0.2 : 0.4;

        //Place sound Forward
        angle = 0.0;

        BuildStream();
    }

    private void BuildStream()
    {
        IEnumerable<IBGCStream> chord = Chord.Select(
            note => GetNote(fundamentalFreq * note.freqRatio, Duration));

        stream = new StreamAdder(chord)
            .Spatialize(angle)
            .Normalize(Level);
    }
}
