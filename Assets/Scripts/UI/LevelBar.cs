using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BGC.MonoUtility.Interpolation;

public class LevelBar : MonoBehaviour
{
    [SerializeField]
    private Text levelText = null;
    [SerializeField]
    private Image levelBarOverlay = null;
    [SerializeField]
    private GameObject levelUpPrefab = null;
    [SerializeField]
    private GameObject levelDownPrefab = null;

    private int displayedLevel = 1;
    private float currentLevel = 0f;
    private const float RATE_PER_SEC = 1f;

    public void SetCurrentLevel(float level)
    {
        currentLevel = level;
        displayedLevel = Mathf.FloorToInt(currentLevel);
        UpdateDisplay();
    }

    public void RunToLevel(float level, Action runCompleteCallback = null)
    {
        StartCoroutine(_RunToLevel(level, runCompleteCallback));
    }

    private IEnumerator _RunToLevel(float targetLevel, Action runCompleteCallback)
    {
        float sign = currentLevel < targetLevel ? 1f : -1f;

        yield return null;

        while (currentLevel != targetLevel)
        {
            float delta = RATE_PER_SEC * Time.deltaTime;
            if (delta < sign*(targetLevel - currentLevel))
            {
                //Normal step
                currentLevel += sign * delta;
            }
            else
            {
                //Final Step
                currentLevel = targetLevel;
            }

            //Test For Level Up
            if (displayedLevel != Mathf.FloorToInt(currentLevel))
            {
                int newLevel = Mathf.FloorToInt(currentLevel);
                GameObject levelChangeIndicator;
                Vector2 drift;
                if (displayedLevel < newLevel)
                {
                    drift = 60f * Vector2.up;
                    levelChangeIndicator = Instantiate(
                        levelUpPrefab, levelText.transform, false);
                }
                else
                {
                    drift = 60f * Vector2.down;
                    levelChangeIndicator = Instantiate(
                        levelDownPrefab, levelText.transform, false);
                }

                displayedLevel = newLevel;
                levelChangeIndicator
                    .AddComponent<LerpGameObjectChannel>()
                    .Activate(
                        duration: 2f,
                        continuousAction: new ContinuousTranslation(drift),
                        finishedCallback: (GameObject obj) => Destroy(obj));
            }

            UpdateDisplay();

            yield return null;
        }

        runCompleteCallback?.Invoke();
    }

    public void UpdateDisplay()
    {
        levelText.text = displayedLevel.ToString();
        
        levelBarOverlay.rectTransform.anchorMax = new Vector2(
            x: Mathf.Repeat(currentLevel, 1.0f),
            y: 1.0f);
    }
}
