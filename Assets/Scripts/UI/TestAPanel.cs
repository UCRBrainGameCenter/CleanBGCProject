using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BGC.UI.Panels;
using BGC.UI.Dialogs;
using BGC.Audio;

public class TestAPanel : ModePanel
{
    [Header("Engine References")]
    [SerializeField]
    private MenuManager menuManager = null;

    [Header("Interface References")]
    [SerializeField]
    private Button quitButton = null;
    [SerializeField]
    private Transform feedbackContainer = null;

    [Header("Audio References")]
    [SerializeField]
    private BGCClipPlayer streamPlayer = null;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject smilieFace = null;
    [SerializeField]
    private GameObject xMark = null;

    private void Awake()
    {
        quitButton.onClick.AddListener(QuitTaskClicked);
    }

    public override void FocusAcquired()
    {
        //Start task
    }

    public override void FocusLost()
    {
        //Clean Up Task
    }

    private void QuitTask()
    {
        menuManager.PopWindowState();
    }

    private void QuitTaskClicked()
    {
        ModalDialog.ShowSimpleModal(ModalDialog.Mode.ConfirmCancel,
            headerText: "Quit Task",
            bodyText: "Are you sure you want to quit the task?",
            callback: (response) =>
            {
                switch (response)
                {
                    case ModalDialog.Response.Confirm:
                        QuitTask();
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
