namespace TestB
{
    public interface ITestBMessenger
    {
        bool PlayerResponded { get; set; }

        bool IsTaskComplete();

        void ResetData();

        void PresentTrialButton();
        void HideTrialButton();

        void SubmitTrial(bool correct);
        void SetCompletedScreen(bool show);
    }
}
