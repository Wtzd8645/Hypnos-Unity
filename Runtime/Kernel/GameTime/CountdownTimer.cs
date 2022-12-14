using System;

namespace Morpheus.GameTime
{
    public class CountdownTimer : GameTimerBase
    {
        protected Action<CountdownTimer> OnTimeUp;

        public void Set(float time, Action<CountdownTimer> onTimeUpCb)
        {
            Interval = time;
            LeftTime = time;
            OnTimeUp = onTimeUpCb;
        }

        public override void Reset()
        {
            IsStop = true;
            OnTimeUp = null;
        }

        public override void Restart()
        {
            IsStop = false;
            LeftTime = Interval;
        }

        internal override void Tick(GameTimeInfo timeInfo)
        {
            LeftTime -= timeInfo.DeltaTime;
            if (LeftTime > 0f)
            {
                return;
            }

            IsStop = true;
            OnTimeUp?.Invoke(this);
        }

        public void Add(float deltaTime)
        {
            LeftTime += deltaTime;
        }

        public override void Scale(float additiveIncreaseRatio)
        {
            if (additiveIncreaseRatio < 0f) // Accelerate
            {
                LeftTime /= 1f - additiveIncreaseRatio;
            }
            else // Decelerate
            {
                LeftTime *= 1f + additiveIncreaseRatio;
            }
        }
    }
}