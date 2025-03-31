namespace Blanketmen.Hypnos
{
    public abstract class GameTimerBase
    {
        internal GameTimerBase nextNode;

        protected float interval;

        public bool IsStop { get; protected set; } = true;

        public float LeftTime { get; protected set; }

        public abstract void Reset();

        public abstract void Restart();

        public void Start()
        {
            IsStop = false;
            GameTimeManager.Instance.AddLast(this);
        }

        public void Stop()
        {
            IsStop = true;
        }

        internal abstract void Tick(GameTimeInfo timeInfo);

        /// <summary>
        /// Positive is Decelerate; Negative number is acceleration.
        /// For example: 0.8f equals +80% current time and -0.5f equals -50% current time.
        /// </summary>
        /// <param name="additiveIncreaseRatio"></param>
        public abstract void Scale(float additiveIncreaseRatio);
    }
}