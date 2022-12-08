using System;

namespace Morpheus.GameTime
{
    public class LapTimer : GameTimerBase
    {
        protected Action<LapTimer> onLapTimeUp;
        protected Action<LapTimer> onTimeUp;

        private float scaledInterval;
        private float lapLeftTime;
        private int totalLap;
        private int leftLap;

        public void Set(float time, int laps, Action<LapTimer> onLapTimeUpCb, Action<LapTimer> onTimeUpCb)
        {
            LeftTime = time * laps;
            interval = time;
            scaledInterval = time;
            lapLeftTime = time;
            totalLap = laps;
            leftLap = laps;
            onLapTimeUp = onLapTimeUpCb;
            onTimeUp = onTimeUpCb;
        }

        public override void Reset()
        {
            IsStop = true;
            onLapTimeUp = null;
            onTimeUp = null;
        }

        public override void Restart()
        {
            IsStop = false;
            LeftTime = interval * totalLap;
            scaledInterval = interval;
            lapLeftTime = interval;
            leftLap = totalLap;
        }

        internal override void Tick(GameTimeInfo timeInfo)
        {
            LeftTime -= timeInfo.deltaTime;
            lapLeftTime -= timeInfo.deltaTime;
            if (lapLeftTime > 0f)
            {
                return;
            }

            lapLeftTime += scaledInterval;
            onLapTimeUp?.Invoke(this);

            if (--leftLap < 1)
            {
                IsStop = true;
                onTimeUp?.Invoke(this);
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