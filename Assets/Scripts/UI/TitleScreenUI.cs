using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BGC.Users;
using BGC.Study;
using BGC.UI.Dialogs;
using BGC.UI.Panels;

public class TitleScreenUI : ModePanel
{
    [Header("Engine Reference")]
    [SerializeField]
    private MenuManager menuManager = null;

    [Header("Title Screen Elements")]
    [SerializeField]
    private Button settingsButton = null;
    [SerializeField]
    private Button quitButton = null;

    [Header("Test Buttons")]
    [SerializeField]
    private Button testAButton = null;
    [SerializeField]
    private Button testBButton = null;
    [SerializeField]
    private Button soundTestButton = null;
    [SerializeField]
    private Button fileBrowserButton = null;

    void Awake()
    {
        settingsButton.onClick.AddListener(() => menuManager.SetWindowState(MenuManager.WindowState.Settings));

        testAButton.onClick.AddListener(menuManager.RunTestA);
        testBButton.onClick.AddListener(menuManager.RunTestB);
        soundTestButton.onClick.AddListener(() => SceneManager.LoadScene("SoundTestScene"));
        fileBrowserButton.onClick.AddListener(() => SceneManager.LoadScene("FileBrowser"));

        BGC.Utility.FileBrowser.FileBrowser.ReturnToScene = "MenuScene";

        quitButton.onClick.AddListener(QuitClicked);

#if UNITY_STANDALONE
        quitButton.gameObject.SetActive(true);
#else
        quitButton.gameObject.SetActive(false);
#endif
    }

#if UNITY_STANDALONE
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ModalDialog.ShowSimpleModal(ModalDialog.Mode.ConfirmCancel,
                headerText: "Quit?",
                bodyText: "Are you sure you want to quit?",
                callback: (ModalDialog.Response response) =>
                {
                    switch (response)
                    {
                        case ModalDialog.Response.Confirm:
                            Application.Quit();
                            break;

                        case ModalDialog.Response.Cancel:
                            //Do Nothing
                            break;

                        case ModalDialog.Response.Accept:
                        default:
                            Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                            break;
                    }
                });
        }
    }
#endif

    #region ModePanel

    public override void FocusAcquired()
    {
        settingsButton.gameObject.SetActive(!PlayerData.IsLocked);

        PlayerData.Save();
    }

    public override void FocusLost()
    {
        // Do Nothing
    }

    #endregion ModePanel
    #region Callbacks

    public void QuitClicked() => Application.Quit();

    #endregion Callbacks
}
