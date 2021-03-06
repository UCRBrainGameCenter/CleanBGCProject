﻿using BGC.StateMachine;

namespace TestB
{
    public class ResponseWindowState : TriggeringState<StateTrigger>
    {
        protected override string DefaultName => "ResponseWindowState";
        protected readonly ITestBMessenger messenger;
        private float timeOutTime;

        public ResponseWindowState(ITestBMessenger messenger)
        {
            this.messenger = messenger;
        }

        protected override void OnStateEnter()
        {
            messenger.PlayerResponded = false;
            timeOutTime = UnityEngine.Time.time + 3f;

            //Show Target Button
            messenger.PresentTrialButton();
        }

        protected override void OnStateExit()
        {
            messenger.HideTrialButton();
        }

        public override void Update()
        {
            if (messenger.PlayerResponded)
            {
                messenger.SubmitTrial(true);
                ActivateTrigger(StateTrigger.NextState);
            }
            else if (UnityEngine.Time.time > timeOutTime)
            {
                messenger.SubmitTrial(false);
                ActivateTrigger(StateTrigger.NextState);
            }
        }
    }
}
