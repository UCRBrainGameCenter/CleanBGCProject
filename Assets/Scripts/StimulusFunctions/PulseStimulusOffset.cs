using System;
using UnityEngine;
using BGC.MonoUtility.Interpolation;

public class PulseStimulusOffset : IContinuousAction<VisualStimulus>
{
    private readonly float deltaTime;
    private float nextTime;

    private VisualStimulus visualStimulus;

    public PulseStimulusOffset(float deltaTime)
    {
        this.deltaTime = deltaTime;
        nextTime = 0f;
    }

    void IContinuousAction<VisualStimulus>.Initialize(VisualStimulus stim, float time)
    {
        visualStimulus = stim;

        nextTime = time + deltaTime;
    }

    void IContinuousAction<VisualStimulus>.CallAction(float time)
    {
        if (time > nextTime)
        {
            nextTime = time + deltaTime;

            float offset = visualStimulus.Offset;

            if (offset >= 0)
            {
                offset -= 0.5f;
            }
            else
            {
                offset += 0.5f;
            }

            visualStimulus.Offset = offset;
        }
    }
}