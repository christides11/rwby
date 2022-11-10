using TMPro;

namespace rwby
{
    public class TimerHUDElement : HUDElement
    {
        public TextMeshProUGUI timer;

        public ITimeProvider timeProvider;

        private int lastTime = -1;
        
        public override void InitializeElement(BaseHUD parentHUD)
        {
            timeProvider = (ITimeProvider)GameModeBase.singleton;
        }

        public override void UpdateElement(BaseHUD parentHUD)
        {
            SetTime(timeProvider.GetTimeInSeconds());
        }

        public void SetTime(int totalSeconds)
        {
            if (totalSeconds == lastTime) return;
            lastTime = totalSeconds;
            int minutes = totalSeconds / 60;
            int seconds = (totalSeconds % 60);
            timer.text = $"{minutes:0}:{seconds:00}";
        }
    }
}