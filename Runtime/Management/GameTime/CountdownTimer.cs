using System;

namespace Blanketmen.Hypnos
{
    public class CountdownTimer : GameTimerBase
    {
        protected Action<CountdownTimer> onTimeUp;

        public void Set(float time, Action<CountdownTimer> onTimeUpCb)
        {
            interval = time;
            LeftTime = time;
            onTimeUp = onTimeUpCb;
        }

        public override void Reset()
        {
            IsStop = true;
            onTimeUp = null;
        }

        public override void Restart()
        {
            IsStop = false;
            LeftTime = interval;
        }

        internal override void Tick(GameTimeInfo timeInfo)
        {
            LeftTime -= timeInfo.deltaTime;
            if (LeftTime > 0f)
            {
                return;
            }

            IsStop = true;
            onTimeUp?.Invoke(this);
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