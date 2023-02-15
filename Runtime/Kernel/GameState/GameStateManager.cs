using System;
using System.Collections;
using System.Collections.Generic;

namespace Hypnos.GameState
{
    public sealed class GameStateManager
    {
        public const int NullStateId = 0;

        #region Singleton
        public static GameStateManager Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance ??= new GameStateManager();
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }
        #endregion

        private readonly Dictionary<int, GameStateBase> stateMap = new Dictionary<int, GameStateBase>(11);

        private bool isPlaying;
        private bool isTransiting;
        private int startStateId;
        private GameStateBase currentState;

        public int CurrentStateId => currentState.Id;

        private GameStateManager() { }

        public void Initialize(GameStateConfig config)
        {
            Type attrType = typeof(ClassIdentityAttribute);
            for (int i = 0; i < config.gameStates.Length; ++i)
            {
                GameStateBase state = config.gameStates[i];
                ClassIdentityAttribute attr = Attribute.GetCustomAttribute(state.GetType(), attrType) as ClassIdentityAttribute;
                if (attr == null)
                {
                    Kernel.LogError($"[GameStateManager] GameState has no attribute. Class: {state}");
                    continue;
                }

                if (stateMap.ContainsKey(attr.Id))
                {
                    Kernel.LogError($"[GameStateManager] Duplicated GameState Id: {attr.Id}");
                    continue;
                }

                state.Id = attr.Id;
                stateMap.Add(attr.Id, state);
            }
            startStateId = config.startGameState;
        }

        public void Start()
        {
            if (currentState != null)
            {
                Kernel.LogError($"[GameStateManager] GameStateManager has started.");
                return;
            }

            Kernel.ExecuteCoroutine(StartRoutine());
        }

        private IEnumerator StartRoutine()
        {
            isPlaying = true;
            stateMap.TryGetValue(startStateId, out currentState);
            yield return currentState.OnEnter(NullStateId);

            currentState.OnTransitionComplete();
            currentState.enabled = true;
        }

        public void Switch(int stateId)
        {
            if (isTransiting)
            {
                Kernel.LogError($"[GameStateManager] GameState is transiting. TargetState: {stateId}");
                return;
            }

            Kernel.ExecuteCoroutine(TransitRoutine(stateId));
        }

        private IEnumerator TransitRoutine(int nextStateId)
        {
            isTransiting = true;
            currentState.enabled = false;
            yield return Kernel.WaitForEndOfFrame; // NOTE: Wait for the update of the current frame to complete.

            int prevStateId = currentState.Id;
            yield return currentState.OnExit(nextStateId);

            stateMap.TryGetValue(nextStateId, out currentState);
            yield return currentState.OnEnter(prevStateId);

            isTransiting = false;
            currentState.OnTransitionComplete();
            currentState.enabled = isPlaying;
        }

        public void Play()
        {
            isPlaying = true;
            if (isTransiting || currentState == null)
            {
                return;
            }

            currentState.enabled = true;
        }

        public void Pause()
        {
            isPlaying = false;
            if (isTransiting || currentState == null)
            {
                return;
            }

            currentState.enabled = false;
        }
    }
}