namespace Hypnos.GameTime
{
    public class UnscaledCountdownTimer : CountdownTimer
    {
        internal override void Tick(GameTimeInfo timeInfo)
        {
            LeftTime -= timeInfo.unscaledDeltaTime;
            if (LeftTime > 0f)
            {
                return;
            }

            IsStop = true;
            onTimeUp?.Invoke(this);
        }
    }
}