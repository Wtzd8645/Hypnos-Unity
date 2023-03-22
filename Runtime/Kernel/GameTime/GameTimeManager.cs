using UnityEngine;

namespace Blanketmen.Hypnos
{
    public sealed class GameTimeManager
    {
        #region Singleton
        public static GameTimeManager Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance ??= new GameTimeManager();
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }
        #endregion

        private static readonly GameTimeInfo timeInfo = new GameTimeInfo();

        public static float RealTimeSinceStartup => timeInfo.realTimeSinceStartup;
        public static float FixedDeltaTime => timeInfo.fixedDeltaTime;
        public static float UnscaledDeltaTime => timeInfo.unscaledDeltaTime;
        public static float DeltaTime => timeInfo.deltaTime;

        // NOTE:
        // Stack pool makes memory fragmentation(I guess),
        // and it'll take 5 times longer than use new objects in 1 million quantity.
        // private Dictionary<Type, Stack<TimerBase>> timerPools = new Dictionary<Type, Stack<TimerBase>>(4);

        private GameTimerBase headNode;
        private GameTimerBase tailNode;

        private GameTimeManager() { }

        public void Clear()
        {
            GameTimerBase currNode = headNode;
            while (currNode != null)
            {
                GameTimerBase nextNode = currNode.nextNode;
                currNode.Reset();
                currNode.nextNode = null;
                currNode = nextNode;
            }
            headNode = null;
            tailNode = null;
        }

        public void FixedUpdate()
        {
            timeInfo.fixedDeltaTime = Time.fixedDeltaTime;
        }

        // NOTE: This update MUST BE before other scripts.
        public void Update()
        {
            // NOTE: Access Time.deltaTime in loop is slower.
            timeInfo.realTimeSinceStartup = Time.realtimeSinceStartup;
            timeInfo.unscaledDeltaTime = Time.unscaledDeltaTime;
            timeInfo.deltaTime = Time.deltaTime;

            GameTimerBase prevNode = null;
            GameTimerBase currNode = headNode;
            while (currNode != null)
            {
                if (currNode.IsStop)
                {
                    // Remove node.
                    if (prevNode == null)
                    {
                        headNode = currNode.nextNode; // NOTE: NextNode can't be self.
                    }
                    else
                    {
                        prevNode.nextNode = currNode.nextNode;
                    }

                    // Set last node.
                    if (currNode == tailNode)
                    {
                        tailNode = prevNode;
                    }

                    // Move to next node.
                    GameTimerBase nextNode = currNode.nextNode;
                    currNode.nextNode = null;
                    currNode = nextNode;
                    continue;
                }

                currNode.Tick(timeInfo);
                prevNode = currNode;
                currNode = currNode.nextNode;
            }
        }

        internal void AddLast(GameTimerBase timer)
        {
            if (timer.nextNode != null || timer == tailNode)
            {
                return;
            }

            if (headNode == null)
            {
                headNode = timer;
                tailNode = timer;
            }
            else
            {
                tailNode.nextNode = timer;
                tailNode = timer;
            }
        }
    }
}