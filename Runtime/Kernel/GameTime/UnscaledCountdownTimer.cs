namespace Morpheus.GameTime
{
    public class UnscaledCountdownTimer : CountdownTimer
    {
        internal override void Tick(GameTimeInfo timeInfo)
        {
            LeftTime -= timeInfo.UnscaledDeltaTime;
            if (LeftTime > 0f)
            {
                return;
            }

            IsStop = true;
            OnTimeUp?.Invoke(this);
        }
    }
}