using UnityEngine;
using BGC.Mathematics;
using BGC.StateMachine;

namespace TestB
{
    /// <summary>
    /// InterTrialInterval state - the delay between trials
    /// </summary>
    public class ITIState : State
    {
        protected override string DefaultName => "ITIState";
        protected readonly ITestBMessenger messenger;

        public ITIState(ITestBMessenger messenger)
        {
            this.messenger = messenger;
        }

        private float endTime;

        protected override void OnStateEnter() => endTime = Time.time + 0.25f + CustomRandom.NextFloat();

        public override void Update()
        {
            if (Time.time > endTime)
            {
                ActivateTrigger(StateKeys.NextStateTrigger);
            }
        }
    }
}
