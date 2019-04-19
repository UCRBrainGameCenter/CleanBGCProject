using UnityEngine;
using UnityEngine.UI;
using BGC.MonoUtility.Interpolation;

namespace TestA
{
    /// <summary>
    /// Part of the Feature to mess with TestA.
    /// Lerps an image's color from the initial to the target color.
    /// </summary>
    public class ImageColorDrifter : ILerpAction<SettingsDemo>
    {
        private readonly Image imageElement;
        private readonly Color targetColor;
        private Color initialColor;

        public ImageColorDrifter(Image imageElement, Color targetColor)
        {
            this.imageElement = imageElement;
            this.targetColor = targetColor;
        }
        void ILerpAction<SettingsDemo>.Initialize(SettingsDemo target)
        {
            initialColor = imageElement.color;
        }

        void ILerpAction<SettingsDemo>.CallAction(float t)
        {
            imageElement.color = Color.Lerp(initialColor, targetColor, t);
        }
    }
}
