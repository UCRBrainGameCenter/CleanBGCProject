using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BGC.Users;
using BGC.Study;
using BGC.Utility;
using BGC.Web;
using BGC.MonoUtility;
using BGC.UI.Dialogs;
using BGC.UI.Panels;

public class MenuManager : MonoBehaviour
{
    [Header("Engine References")]
    [SerializeField]
    private ModePanelManager panelManager = null;
    [SerializeField]
    private TitleScreenUI titleScreenManager = null;
    [SerializeField]
    private SettingsInnerPanel settingsMenu = null;
    [SerializeField]
    private TestAPanel testAPanel = null;
    [SerializeField]
    private TestBPanel testBPanel = null;
    [SerializeField]
    private StudyMessage messageMenu = null;

    public enum WindowState
    {
        Title = 0,
        Settings,
        TestA,
        TestB,
        Message,
        MAX
    }

    private WindowState currentWindowState;

    private Stack<WindowState> windowStack = new Stack<WindowState>();

    void Awake()
    {
        GetComponent<RectMask2D>().enabled = true;
    }

    void Start()
    {
        currentWindowState = WindowState.Settings;
        panelManager.ImmediatePanelSet(settingsMenu);
    }

    public void SetWindowState(WindowState state, ShowPanelMode mode = ShowPanelMode.Hierarchy)
    {
        ModePanel newPanel = GetModePanel(state);

        if (newPanel != null)
        {
            currentWindowState = state;
            panelManager.SetPanelActive(newPanel, mode);
        }
    }

    public void FlashToWindowState(WindowState state)
    {
        ModePanel newPanel = GetModePanel(state);

        if (newPanel != null)
        {
            currentWindowState = state;
            panelManager.ImmediatePanelSet(newPanel);
        }
    }

    /// <summary>
    /// Moves the curernt state onto the window stack and sets the current stat to <paramref name="state"/>
    /// </summary>
    /// <param name="state"></param>
    public void PushWindowState(WindowState state)
    {
        windowStack.Push(currentWindowState);
        SetWindowState(state, ShowPanelMode.Push);
    }

    /// <summary>
    /// Sets the current window state to the last one on the stack.
    /// </summary>
    /// <param name="abort"></param>
    public void PopWindowState()
    {
        SetWindowState(windowStack.Pop(), ShowPanelMode.Pop);
    }

    public void RunTestA() => PushWindowState(WindowState.TestA);

    public void RunTestB() => PushWindowState(WindowState.TestB);

    /// <summary>
    /// Clear the accumulated window stack
    /// </summary>
    public void ClearWindowStack()
    {
        windowStack.Clear();
    }

    #region Helper Methods

    private ModePanel GetModePanel(WindowState state)
    {
        switch (state)
        {
            case WindowState.Title: return titleScreenManager;
            case WindowState.Settings: return settingsMenu;
            case WindowState.TestA: return testAPanel;
            case WindowState.TestB: return testBPanel;
            case WindowState.Message: return messageMenu;

            default:
                Debug.LogError($"WindowState Unimplemented: {state}");
                return null;
        }
    }

    #endregion Helper Methods
}