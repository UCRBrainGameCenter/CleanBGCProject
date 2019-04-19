using UnityEngine;
using BGC.Mathematics;
using BGC.MonoUtility.Interpolation;

namespace TestA
{
    /// <summary>
    /// Part of the Feature to mess with TestA.
    /// Causes a frame to randomly drift around
    /// </summary>
    public class GameObjectDrifter : IContinuousAction<SettingsDemo>
    {
        private readonly RectTransform driftTarget;
        private readonly float driftLength;

        public GameObjectDrifter(RectTransform driftTarget, float driftLength)
        {
            this.driftTarget = driftTarget;
            this.driftLength = driftLength;
        }
        void IContinuousAction<SettingsDemo>.Initialize(SettingsDemo target, float time) { }

        void IContinuousAction<SettingsDemo>.CallAction(float time)
        {
            float theta = 2 * Mathf.PI * CustomRandom.NextFloat();
            driftTarget.localPosition += driftLength * new Vector3(
                x: Mathf.Cos(theta),
                y: Mathf.Sin(theta),
                z: 0f);
        }
    }
}
