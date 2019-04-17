using UnityEngine.UI;
using UnityEngine;
using BGC.Study;
using BGC.UI.Panels;

public class StudyMessage : ModePanel
{
    [Header("Engine References")]
    [SerializeField]
    private MenuManager menuManager = null;

    [Header("UI Elements")]
    [SerializeField]
    private Text messageText = null;
    [SerializeField]
    private GameObject timerPanel = null;
    [SerializeField]
    private Text timerText = null;
    [SerializeField]
    private InputField passwordInputField = null;
    [SerializeField]
    private Button continueButton = null;
    [SerializeField]
    private Text continueButtonText = null;

    private string message = "Congratulations";
    private string buttonText = "Continue";
    private bool requirePassword = false;
    private bool blockManualContinue = false;
    private bool autoProgress = false;
    private float timeLimit = 5.0f;
    private bool showTimer = true;

    private bool breakActive = false;

    private float endTime = 0f;

    private int lastSecondCount = -1;

    #region Monobehaviour

    void Awake()
    {
        continueButton.onClick.AddListener(ContinueButtonPressed);
        passwordInputField.onEndEdit.AddListener(EditEnded);
    }

    private void OnDestroy()
    {
        continueButton.onClick.RemoveListener(ContinueButtonPressed);
        passwordInputField.onEndEdit.RemoveListener(EditEnded);
    }

    void Update()
    {
        if (breakActive)
        {
            if (showTimer)
            {
                UpdateTimerDisplay();
            }

            if (autoProgress)
            {
                if (Time.time > endTime)
                {
                    ContinueButtonPressed();
                }
            }
        }
    }

    #endregion Monobehaviour
    #region ModePanel

    public override void FocusAcquired()
    {
        continueButton.gameObject.SetActive(!blockManualContinue);
        continueButton.interactable = !requirePassword;

        continueButtonText.text = buttonText;

        passwordInputField.gameObject.SetActive(requirePassword);

        messageText.text = message;

        timerPanel.SetActive(showTimer);

        lastSecondCount = -1;
        endTime = Time.time + timeLimit;

        breakActive = true;
    }

    public override void FocusLost()
    {
        breakActive = false;
    }

    #endregion ModePanel
    #region Callbacks

    protected void ContinueButtonPressed()
    {
        breakActive = false;
        menuManager.PopWindowState();
    }

    protected void EditEnded(string text)
    {
        if (text == "3141")
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
            passwordInputField.gameObject.SetActive(false);
        }
    }

    #endregion Callbacks
    #region Helper Methods

    protected void UpdateTimerDisplay()
    {
        float remainingTime = endTime - Time.time;

        if (remainingTime <= 0f)
        {
            timerText.text = "00:00";
            return;
        }

        int timeSecs = Mathf.FloorToInt(remainingTime);

        if (timeSecs != lastSecondCount)
        {
            lastSecondCount = timeSecs;
            timerText.text = 
                $"{(lastSecondCount / 60).ToString("D2")}:{(lastSecondCount % 60).ToString("D2")}";
        }

    }

    #endregion Helper Methods
}
