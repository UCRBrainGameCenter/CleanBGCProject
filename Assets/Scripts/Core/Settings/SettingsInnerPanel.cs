using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BGC.Users;
using BGC.UI.Dialogs;
using BGC.UI.Panels;

public class SettingsInnerPanel : ModePanel
{
    [Header("Engine References")]
    [SerializeField]
    private ModePanelManager settingsPanelManager = null;
    [SerializeField]
    private SettingsMenu generalSettingsMenu = null;
    [SerializeField]
    private UserIDMenu userIDMenu = null;


    [Header("Settings Menu Tab Buttons")]
    [SerializeField]
    private Button generalSettingsButton = null;
    [SerializeField]
    private Button userSelectButton = null;

    [Header("Settings Menu UI")]
    [SerializeField]
    private Button lockButton = null;

    private Color SelectedButtonColor => Color.black;
    private Color SelectedTextColor => Color.white;
    private Color EnabledButtonColor => Color.gray;
    private Color EnabledTextColor => Color.white;
    private Color DisabledButtonColor => Color.gray;
    private Color DisabledTextColor => Color.black;
    private Color HighlightButtonColor => Color.green;
    private Color HighlightTextColor => Color.black;

    protected enum SettingPanelMode
    {
        General = 0,
        UserSelect,
        MAX
    }

    protected enum TabButtonState
    {
        Selected = 0,
        Enabled,
        Disabled,
        Hidden,
        Flashing,
        MAX
    }

    protected SettingPanelMode currentMode;
    protected bool requestedLockInteractivity = true;
    protected bool flashOn = false;

    void Awake()
    {
        generalSettingsButton.onClick.AddListener(() => TabButtonClicked(SettingPanelMode.General));
        userSelectButton.onClick.AddListener(() => TabButtonClicked(SettingPanelMode.UserSelect));
        lockButton.onClick.AddListener(LockPressed);
    }

    #region ModePanel

    public override void FocusAcquired()
    {
        flashOn = false;
        if (PlayerData.IsDefault)
        {
            settingsPanelManager.ImmediatePanelSet(userIDMenu);
            UpdateUIForMode(SettingPanelMode.UserSelect);

            StartCoroutine(FlashUserButton());
        }
        else
        {
            settingsPanelManager.ImmediatePanelSet(generalSettingsMenu);
            UpdateUIForMode(SettingPanelMode.General);
        }
    }

    public override void FocusLost() { }

    #endregion ModePanel

    protected void LockPressed()
    {
        if (PlayerData.IsLocked)
        {
            ModalDialog.ShowInputModal(
                mode: ModalDialog.Mode.InputConfirmCancel,
                headerText: "Enter Code",
                bodyText: "Enter the unlock code.\n\n" +
                    "Note: This enables experimental features.\n" +
                    "The Code is 3141",
                inputCallback: LockSubmit);
        }
        else
        {
            PlayerData.IsLocked = true;

            switch (currentMode)
            {
                case SettingPanelMode.General:
                    generalSettingsMenu.LockStateChanged();
                    break;

                case SettingPanelMode.UserSelect:
                    userIDMenu.LockStateChanged();
                    break;

                default:
                    Debug.LogError($"Unexpected mode: {currentMode}");
                    break;
            }

            UpdateUIForMode(currentMode);
        }
    }

    protected void LockSubmit(ModalDialog.Response response, string input)
    {
        switch (response)
        {
            case ModalDialog.Response.Confirm:
                if (input == "3141")
                {
                    PlayerData.IsLocked = false;
                }
                else
                {
                    ModalDialog.ShowSimpleModal(
                        mode: ModalDialog.Mode.Accept,
                        headerText: "Incorrect Code",
                        bodyText: "The entered code was incorrect.");
                }
                break;

            case ModalDialog.Response.Cancel:
                //Do nothing
                break;

            default:
                Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                break;
        }

        switch (currentMode)
        {
            case SettingPanelMode.General:
                generalSettingsMenu.LockStateChanged();
                break;

            case SettingPanelMode.UserSelect:
                userIDMenu.LockStateChanged();
                break;

            default:
                Debug.LogError($"Unexpected mode: {currentMode}");
                break;
        }

        UpdateUIForMode(currentMode);
    }

    protected void TabButtonClicked(SettingPanelMode mode)
    {
        UpdateUIForMode(mode);

        switch (mode)
        {
            case SettingPanelMode.General:
                settingsPanelManager.SetPanelActive(generalSettingsMenu);
                break;

            case SettingPanelMode.UserSelect:
                settingsPanelManager.SetPanelActive(userIDMenu);
                break;

            default:
                Debug.LogError($"Unexpected SettingPanelMode: {mode}");
                break;
        }
    }

    protected void UpdateUIForMode(SettingPanelMode mode)
    {
        currentMode = mode;

        lockButton.GetComponentInChildren<Text>().text = PlayerData.IsLocked ? "Unlock" : "Lock";

        lockButton.interactable =
            mode == SettingPanelMode.General ||
            mode == SettingPanelMode.UserSelect;

        UpdateButtonState(generalSettingsButton, SettingPanelMode.General);
        UpdateButtonState(userSelectButton, SettingPanelMode.UserSelect);
    }

    protected void UpdateButtonState(Button button, SettingPanelMode mode)
    {
        TabButtonState state = GetButtonState(mode);

        //Set button interactable state
        switch (state)
        {
            case TabButtonState.Enabled:
            case TabButtonState.Flashing:
                button.gameObject.SetActive(true);
                button.interactable = true;
                break;

            case TabButtonState.Disabled:
            case TabButtonState.Selected:
                button.gameObject.SetActive(true);
                button.interactable = false;
                break;

            case TabButtonState.Hidden:
                button.gameObject.SetActive(false);
                break;

            default:
                Debug.LogError($"Unexpected TabButtonState: {state}");
                break;
        }

        button.GetComponent<Image>().color = GetButtonColor(state);
        button.GetComponentInChildren<Text>().color = GetTextColor(state);

    }

    protected TabButtonState GetButtonState(SettingPanelMode button)
    {
        if (button == currentMode)
        {
            return TabButtonState.Selected;
        }

        switch (button)
        {
            case SettingPanelMode.General:
                return TabButtonState.Enabled;

            case SettingPanelMode.UserSelect:
                if (PlayerData.IsDefault || !PlayerData.IsLocked)
                {
                    return flashOn ? TabButtonState.Flashing : TabButtonState.Enabled;
                }
                return TabButtonState.Disabled;

            default:
                Debug.LogError($"Unexpected SettingPanelMode: {button}");
                return TabButtonState.Enabled;
        }
    }

    protected Color GetButtonColor(TabButtonState state)
    {
        switch (state)
        {
            case TabButtonState.Selected: return SelectedButtonColor;
            case TabButtonState.Enabled: return EnabledButtonColor;

            case TabButtonState.Disabled:
            case TabButtonState.Hidden: return DisabledButtonColor;
            case TabButtonState.Flashing: return HighlightButtonColor;

            default:
                Debug.LogError($"Unexpected TabButtonState: {state}");
                return EnabledButtonColor;
        }
    }

    protected Color GetTextColor(TabButtonState state)
    {
        switch (state)
        {
            case TabButtonState.Selected: return SelectedTextColor;
            case TabButtonState.Enabled: return EnabledTextColor;
            case TabButtonState.Flashing: return HighlightTextColor;

            case TabButtonState.Hidden:
            case TabButtonState.Disabled: return DisabledTextColor;

            default:
                Debug.LogError($"Unexpected TabButtonState: {state}");
                return EnabledButtonColor;
        }
    }

    protected IEnumerator FlashUserButton()
    {
        while (PlayerData.IsDefault)
        {
            flashOn = false;
            userSelectButton.GetComponent<Image>().color =
                GetButtonColor(GetButtonState(SettingPanelMode.UserSelect));
            userSelectButton.GetComponentInChildren<Text>().color =
                GetTextColor(GetButtonState(SettingPanelMode.UserSelect));
            yield return new WaitForSeconds(0.75f);

            //Flash if it's not currently selected
            flashOn = true;
            if (GetButtonState(SettingPanelMode.UserSelect) != TabButtonState.Selected)
            {
                userSelectButton.GetComponent<Image>().color = HighlightButtonColor;
                userSelectButton.GetComponentInChildren<Text>().color = HighlightTextColor;
            }
            yield return new WaitForSeconds(0.75f);
        }

        yield break;
    }
}
