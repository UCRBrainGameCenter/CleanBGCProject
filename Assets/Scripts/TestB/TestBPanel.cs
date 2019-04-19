using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BGC.Audio;
using BGC.UI.Panels;
using BGC.UI.Dialogs;
using BGC.Mathematics;
using BGC.StateMachine;
using BGC.MonoUtility.Interpolation;
using TestB;
using BGC.Users;

/// <summary>
/// In TestB, a button bounces around the screen and must be pressed 5 times to success.
/// Failure to press it for 3 consecutive seconds will cause the player to lose.
/// </summary>
public class TestBPanel : ModePanel, ITestBMessenger
{
    [Header("Engine References")]
    [SerializeField]
    private MenuManager menuManager = null;

    [Header("Interface References")]
    [SerializeField]
    private Button gameButton = null;
    [SerializeField]
    private Button quitButton = null;
    [SerializeField]
    private Button restartButton = null;
    [SerializeField]
    private Transform feedbackContainer = null;
    [SerializeField]
    private Text completedText = null;
    [SerializeField]
    private Text totalStats = null;

    [Header("Audio References")]
    [SerializeField]
    private BGCClipPlayer streamPlayer = null;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject smilingFace = null;
    [SerializeField]
    private GameObject xMark = null;

    private RectTransform panelRectTransform;
    private RectTransform buttonRectTransform;
    private Vector2 panelSize;
    private Vector2 buttonSize;

    private StateMachine gameStateMachine;

    private bool playerResponded = false;
    private int hits = 0;
    private int misses = 0;

    private int TotalHits
    {
        get => PlayerData.GetInt("TaskB.Hits", 0);
        set => PlayerData.SetInt("TaskB.Hits", value);
    }

    private int TotalMisses
    {
        get => PlayerData.GetInt("TaskB.Misses", 0);
        set => PlayerData.SetInt("TaskB.Misses", value);
    }

    private int TotalRuns
    {
        get => PlayerData.GetInt("TaskB.Runs", 0);
        set => PlayerData.SetInt("TaskB.Runs", value);
    }

    private int TotalCompletions
    {
        get => PlayerData.GetInt("TaskB.Completions", 0);
        set => PlayerData.SetInt("TaskB.Completions", value);
    }

    void Awake()
    {
        quitButton.onClick.AddListener(QuitTaskClicked);
        restartButton.onClick.AddListener(RestartTaskClicked);
        gameButton.onClick.AddListener(GameButtonClicked);

        panelRectTransform = GetComponent<RectTransform>();
        buttonRectTransform = gameButton.GetComponent<RectTransform>();

        //Demonstrating a third-party constructing a state machine from a messaging interface
        gameStateMachine = ConstructStateMachine(this);
    }

    private void Update()
    {
        gameStateMachine.Update();
    }

    public override void FocusAcquired()
    {
        //Prepare and Start task
        gameStateMachine.Reset(true);

        //Grab the size of the panel
        panelSize = panelRectTransform.rect.size;
        buttonSize = buttonRectTransform.rect.size;

        gameStateMachine.ActivateTriggerImmediate(StateKeys.NextStateTrigger);
    }

    public override void FocusLost()
    {
        //Clean Up Task

        //Delete hanging feedback elements
        foreach (Transform feedback in feedbackContainer)
        {
            Destroy(feedback.gameObject);
        }
    }

    private void QuitTask()
    {
        //Kill State Machine
        gameStateMachine.ActivateTriggerImmediate(StateKeys.QuitGameTrigger);
        menuManager.PopWindowState();
    }

    private void PresentTrial()
    {
        Vector2 posRange = (panelSize - buttonSize) / 2f;

        buttonRectTransform.localPosition = new Vector2(
            x: posRange.x * (1f - 2f * CustomRandom.NextFloat()),
            y: posRange.y * (1f - 2f * CustomRandom.NextFloat()));
        buttonRectTransform.gameObject.SetActive(true);
    }

    private void ClearTrial()
    {
        buttonRectTransform.gameObject.SetActive(false);
    }

    private void SubmitTrial(bool success)
    {
        streamPlayer.PlayStream(
            new FeedbackChords(success, hits, octaveOffset: 1));

        GameObject feedback;
        if (success)
        {
            hits++;
            TotalHits++;
            feedback = Instantiate(smilingFace, feedbackContainer);
        }
        else
        {
            misses++;
            TotalMisses++;
            feedback = Instantiate(xMark, feedbackContainer);
        }

        feedback.transform.position = buttonRectTransform.position;

        feedback.AddComponent<LerpGameObjectChannel>().Activate(
            duration: 2f,
            continuousAction: new ContinuousTranslation(50 * Vector2.up),
            finishedCallback: gameobj => Destroy(gameobj),
            interruptedCallback: gameobj => Destroy(gameobj));
    }

    private void SetCompletedScreen(bool show)
    {
        if (show)
        {
            TotalCompletions++;

            streamPlayer.PlayStream(Fanfare.LevelCompleteFanfare());

            completedText.text = $"Congratulations.\n" +
                $"Hits: {hits}    Misses: {misses}";

            totalStats.text = $"All Time Stats: " +
                $"{TotalHits}/{TotalHits + TotalMisses} Hits   " +
                $"{TotalCompletions}/{TotalRuns} Completed Runs";
        }

        totalStats.gameObject.SetActive(show);
        completedText.gameObject.SetActive(show);
        restartButton.gameObject.SetActive(show);
    }

    private bool IsTaskComplete() => hits + misses >= 10;

    private static StateMachine ConstructStateMachine(ITestBMessenger messenger)
    {
        StateMachine stateMachine = new StateMachine(true);

        State gameInitState = new LambdaState(
            name: "GameInitState",
            onStateEnter: messenger.HideTrialButton);

        State gamePrepState = new TriggeringLambdaState(
            name: "GamePrepState",
            onStateEnter: () =>
            {
                messenger.ResetData();
                return StateKeys.NextStateTrigger;
            });

        State itiState = new ITIState(messenger);
        State responseWindowState = new ResponseWindowState(messenger);

        State progressControlState = new TriggeringLambdaState(
            name: "ProgressControlState",
            onStateEnter: () =>
            {
                if (messenger.IsTaskComplete())
                {
                    return StateKeys.FinishedTrigger;
                }

                return StateKeys.NextStateTrigger;
            });

        State completedState = new LambdaState(
            name: "CompletedState",
            onStateEnter: () => messenger.SetCompletedScreen(true),
            onStateExit: () => messenger.SetCompletedScreen(false));

        State restartState = new TriggeringLambdaState(
            name: "RestartState",
            onStateEnter: () =>
            {
                PlayerData.Save();
                return StateKeys.NextStateTrigger;
            });

        State quitState = new TriggeringLambdaState(
            name: "QuitState",
            onStateEnter: () =>
            {
                PlayerData.Save();
                return StateKeys.NextStateTrigger;
            });

        stateMachine.AddEntryState(gameInitState);
        stateMachine.AddState(gamePrepState);
        stateMachine.AddState(itiState);
        stateMachine.AddState(responseWindowState);
        stateMachine.AddState(progressControlState);
        stateMachine.AddState(completedState);
        stateMachine.AddState(restartState);
        stateMachine.AddState(quitState);

        //
        // One-To-One State Transitions
        //

        stateMachine.AddTransition(
            fromState: gameInitState,
            targetState: gamePrepState,
            new TriggerCondition(StateKeys.NextStateTrigger));

        stateMachine.AddTransition(
            fromState: gamePrepState,
            targetState: itiState,
            new TriggerCondition(StateKeys.NextStateTrigger));

        stateMachine.AddTransition(
            fromState: responseWindowState,
            targetState: itiState,
            new TriggerCondition(StateKeys.NextStateTrigger));

        stateMachine.AddTransition(
            fromState: itiState,
            targetState: progressControlState,
            new TriggerCondition(StateKeys.NextStateTrigger));

        stateMachine.AddTransition(
            fromState: progressControlState,
            targetState: responseWindowState,
            new TriggerCondition(StateKeys.NextStateTrigger));

        stateMachine.AddTransition(
            fromState: progressControlState,
            targetState: completedState,
            new TriggerCondition(StateKeys.FinishedTrigger));

        stateMachine.AddTransition(
            fromState: restartState,
            targetState: gamePrepState,
            new TriggerCondition(StateKeys.NextStateTrigger));

        stateMachine.AddTransition(
            fromState: quitState,
            targetState: gameInitState,
            new TriggerCondition(StateKeys.NextStateTrigger));

        //
        // Any State Transitions
        //

        stateMachine.AddAnyStateTransition(
            restartState,
            new TriggerCondition(StateKeys.RestartGameTrigger));

        stateMachine.AddAnyStateTransition(
            quitState,
            new TriggerCondition(StateKeys.QuitGameTrigger));

        return stateMachine;
    }

    #region Callbacks

    private void GameButtonClicked() => playerResponded = true;

    private void RestartTaskClicked() =>
        gameStateMachine.ActivateTriggerImmediate(StateKeys.RestartGameTrigger);

    private void QuitTaskClicked()
    {
        ModalDialog.ShowSimpleModal(ModalDialog.Mode.ConfirmCancel,
            headerText: "Quit Task",
            bodyText: "Are you sure you want to quit the test?",
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

    #endregion Callbacks
    #region ITestBManager

    bool ITestBMessenger.PlayerResponded
    {
        get => playerResponded;
        set => playerResponded = value;
    }

    bool ITestBMessenger.IsTaskComplete() => IsTaskComplete();

    void ITestBMessenger.ResetData()
    {
        hits = 0;
        misses = 0;
        playerResponded = false;
        TotalRuns++;
    }

    void ITestBMessenger.PresentTrialButton() => PresentTrial();
    void ITestBMessenger.HideTrialButton() => ClearTrial();

    void ITestBMessenger.SubmitTrial(bool correct) => SubmitTrial(correct);

    void ITestBMessenger.SetCompletedScreen(bool show) => SetCompletedScreen(show);

    #endregion ITestBManager
}
