using UnityEngine;
using UnityEngine.UI;
using BGC.MonoUtility.Interpolation;

public class OscillateImageAlpha : IContinuousAction<GameObject>
{
    private readonly float period;
    private readonly float startingPhase;
    private float startTime;

    private Image image;

    public OscillateImageAlpha(float period, float startingPhase = 0f)
    {
        this.period = period;
        this.startingPhase = startingPhase;
        startTime = 0f;
        image = null;
    }

    void IContinuousAction<GameObject>.Initialize(GameObject gameObject, float time)
    {
        image = gameObject.GetComponent<Image>();

        startTime = time;
    }

    void IContinuousAction<GameObject>.CallAction(float time)
    {
        Color newColor = image.color;
        newColor.a = 0.5f - 0.5f * Mathf.Cos(startingPhase + 2f * Mathf.PI * (time - startTime) / period);
        image.color = newColor;
    }
}