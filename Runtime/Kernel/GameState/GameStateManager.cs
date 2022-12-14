using System;
using System.Collections;
using System.Collections.Generic;

namespace Morpheus.GameState
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
            for (int i = 0; i < config.GameStates.Length; ++i)
            {
                GameStateBase state = config.GameStates[i];
                ClassIdentityAttribute attr = Attribute.GetCustomAttribute(state.GetType(), attrType) as ClassIdentityAttribute;
                if (attr == null)
                {
                    DebugLogger.LogError($"[GameStateManager] GameState has no attribute. Class: {state}");
                    continue;
                }

                if (stateMap.ContainsKey(attr.Id))
                {
                    DebugLogger.LogError($"[GameStateManager] Duplicated GameState Id: {attr.Id}");
                    continue;
                }

                state.Id = attr.Id;
                stateMap.Add(attr.Id, state);
            }
            startStateId = config.StartGameState;
        }

        public void Start()
        {
            if (currentState != null)
            {
                DebugLogger.LogError($"[GameStateManager] GameStateManager has started.");
                return;
            }

            AppKernel.ExecuteCoroutine(StartRoutine());
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
                DebugLogger.LogError($"[GameStateManager] GameState is transiting. TargetState: {stateId}");
                return;
            }

            AppKernel.ExecuteCoroutine(TransitRoutine(stateId));
        }

        private IEnumerator TransitRoutine(int nextStateId)
        {
            isTransiting = true;
            currentState.enabled = false;
            yield return AppKernel.WaitForEndOfFrame; // NOTE: Wait for the update of the current frame to complete.

            int prevStateId = currentState.Id;
            yield return currentState.OnExit(nextStateId);

            stateMap.TryGetValue(nextStateId, out currentState);
            yield return currentState.OnEnter(prevStateId);

            isTransiting = false;
            currentState.OnTransitionComplete();
            currentState.enabled = isPlaying;
        }

        internal void Play()
        {
            isPlaying = true;
            if (isTransiting || currentState == null)
            {
                return;
            }

            currentState.enabled = true;
        }

        internal void Pause()
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