using System;
using UnityEngine;
using BGC.MonoUtility.Interpolation;

/// <summary>
/// Linearly interpolate stimulus contrast
/// </summary>
public class LerpStimulusContrast : ILerpAction<VisualStimulus>
{
    private float initialContrast;
    private float targetContrast;

    private VisualStimulus visualStimulus = null;

    public LerpStimulusContrast(
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
    }

    void ILerpAction<VisualStimulus>.CallAction(float t)
    {
        visualStimulus.Contrast = Mathf.Lerp(initialContrast, targetContrast, t);
    }
}
