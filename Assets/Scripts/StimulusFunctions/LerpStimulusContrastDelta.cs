using System;
using UnityEngine;
using BGC.MonoUtility.Interpolation;

/// <summary>
/// Linearly interpolate stimulus contrast
/// </summary>
public class LerpStimulusContrastDelta : ILerpAction<VisualStimulus>
{
    private readonly float contrastDelta;
    private readonly float contrastRatio;
    private readonly float maxContrast;
    private readonly float minContrast;

    private float initialContrast = 0f;
    private float targetContrast = 0f;

    private VisualStimulus visualStimulus;

    public LerpStimulusContrastDelta(
        float contrastDelta = 0f,
        float contrastRatio = 1f,
        float maxContrast = 2f,
        float minContrast = 0f)
    {
        this.contrastDelta = contrastDelta;
        this.contrastRatio = contrastRatio;
        this.maxContrast = maxContrast;
        this.minContrast = minContrast;
    }

    void ILerpAction<VisualStimulus>.Initialize(VisualStimulus stim)
    {
        visualStimulus = stim;

        initialContrast = Mathf.Clamp(
            value: visualStimulus.Contrast,
            min: minContrast,
            max: maxContrast);

        targetContrast = Mathf.Clamp(
            value: initialContrast * contrastRatio + contrastDelta,
            min: minContrast,
            max: maxContrast);
    }

    void ILerpAction<VisualStimulus>.CallAction(float t)
    {
        visualStimulus.Contrast = Mathf.Lerp(initialContrast, targetContrast, t);
    }
}
