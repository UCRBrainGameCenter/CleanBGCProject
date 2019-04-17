using System;
using UnityEngine;
using BGC.MonoUtility.Interpolation;

/// <summary>
/// Exponentially interpolate stimulus contrast
/// </summary>
public class ExerpStimulusContrast : ILerpAction<VisualStimulus>
{
    private float initialContrast;
    private float targetContrast;
    private float ratio;

    private VisualStimulus visualStimulus;

    public ExerpStimulusContrast(
        float targetContrast,
        float initialContrast = float.NaN)
    {
        this.initialContrast = initialContrast;
        this.targetContrast = targetContrast;
    }

    void ILerpAction<VisualStimulus>.Initialize(VisualStimulus stim)
    {
        visualStimulus = stim;

        if (float.IsNaN(initialContrast))
        {
            initialContrast = visualStimulus.Contrast;
        }

        ratio = targetContrast / initialContrast;
    }

    void ILerpAction<VisualStimulus>.CallAction(float t)
    {
        visualStimulus.Contrast = initialContrast * Mathf.Pow(ratio, Mathf.Clamp01(t));
    }
}
