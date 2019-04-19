using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BGC.MonoUtility.Interpolation;

public class SplashScreenAnimManager : MonoBehaviour
{
    [SerializeField]
    private Image logo = null;
    [SerializeField]
    private string taskScene = "TaskScene";

    private LerpImageChannel channel = null;
    private IEnumerator systemInitializer = null;

    void Start()
    {
        channel = logo.gameObject.AddComponent<LerpImageChannel>();

        Color logoColor = logo.color;
        logoColor.a = 0f;
        logo.color = logoColor;

        systemInitializer = SystemInitializer.Initialize();
        StartCoroutine(systemInitializer);

        channel.Activate(
            duration: 0.5f,
            finishedCallback: FadeIn);
    }

    void FadeIn(Image image)
    {
        channel.Activate(
            duration: 0.75f,
            lerpAction: new FadeImageAlpha(initialAlpha: 0f, finalAlpha: 1f),
            finishedCallback: Wait);
    }

    void Wait(Image image)
    {
        channel.Activate(
            duration: 2f,
            finishedCallback: ReadyForFadeOut);
    }

    void ReadyForFadeOut(Image image)
    {
        if (systemInitializer.MoveNext())
        {
            channel.Activate(
                duration: 0.1f,
                finishedCallback: ReadyForFadeOut);
        }
        else
        {
            channel.Activate(
                duration: 0.5f,
                lerpAction: new FadeImageAlpha(initialAlpha: 1f, finalAlpha: 0f),
                finishedCallback: Wait2);
        }
    }

    void Wait2(Image image)
    {
        channel.Activate(
            duration: 0.1f,
            finishedCallback: LoadScene);
    }

    void LoadScene(Image image)
    {
        SceneManager.LoadSceneAsync(taskScene, LoadSceneMode.Single);
    }
}
