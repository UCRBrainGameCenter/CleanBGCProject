using UnityEngine.UI;
using BGC.Mathematics;
using BGC.MonoUtility.Interpolation;

namespace TestA
{
    /// <summary>
    /// Part of the Feature to mess with TestA.
    /// Randomly swaps adjacent characters every frame
    /// </summary>
    public class TextShuffler : IContinuousAction<SettingsDemo>
    {
        private readonly Text textElement;
        private char[] currentText;

        public TextShuffler(Text textElement)
        {
            this.textElement = textElement;
        }
        void IContinuousAction<SettingsDemo>.Initialize(SettingsDemo target, float time)
        {
            currentText = textElement.text.ToCharArray();
        }

        void IContinuousAction<SettingsDemo>.CallAction(float time)
        {
            //Every Frame, swap adjacent chars
            int index = CustomRandom.Next(0, currentText.Length - 2);
            (currentText[index], currentText[index + 1]) = (currentText[index + 1], currentText[index]);
            textElement.text = new string(currentText);
        }
    }
}
