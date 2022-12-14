using System;

namespace Morpheus.GameTime
{
    public class LapTimer : GameTimerBase
    {
        protected Action<LapTimer> OnLapTimeUp;
        protected Action<LapTimer> OnTimeUp;

        private float scaledInterval;
        private float lapLeftTime;
        private int totalLap;
        private int leftLap;

        public void Set(float time, int laps, Action<LapTimer> onLapTimeUpCb, Action<LapTimer> onTimeUpCb)
        {
            LeftTime = time * laps;
            Interval = time;
            scaledInterval = time;
            lapLeftTime = time;
            totalLap = laps;
            leftLap = laps;
            OnLapTimeUp = onLapTimeUpCb;
            OnTimeUp = onTimeUpCb;
        }

        public override void Reset()
        {
            IsStop = true;
            OnLapTimeUp = null;
            OnTimeUp = null;
        }

        public override void Restart()
        {
            IsStop = false;
            LeftTime = Interval * totalLap;
            scaledInterval = Interval;
            lapLeftTime = Interval;
            leftLap = totalLap;
        }

        internal override void Tick(GameTimeInfo timeInfo)
        {
            LeftTime -= timeInfo.DeltaTime;
            lapLeftTime -= timeInfo.DeltaTime;
            if (lapLeftTime > 0f)
            {
                return;
            }

            lapLeftTime += scaledInterval;
            OnLapTimeUp?.Invoke(this);

            if (--leftLap < 1)
            {
                IsStop = true;
                OnTimeUp?.Invoke(this);
            }
        }

        public void Add(int laps)
        {
            LeftTime += scaledInterval * laps;
            leftLap += laps;
        }

        public override void Scale(float additiveIncreaseRatio)
        {
            if (additiveIncreaseRatio < 0f)
            {
                additiveIncreaseRatio = 1f - additiveIncreaseRatio;
                scaledInterval /= additiveIncreaseRatio;
                lapLeftTime /= additiveIncreaseRatio;
            }
            else
            {
                additiveIncreaseRatio = 1f + additiveIncreaseRatio;
                scaledInterval *= additiveIncreaseRatio;
                lapLeftTime *= additiveIncreaseRatio;
            }

            LeftTime = lapLeftTime + scaledInterval * (leftLap - 1);
        }
    }
}