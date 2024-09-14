using System;
using System.Collections;
using System.Collections.Generic;

namespace Blanketmen.Hypnos
{
    public sealed class GameStateManager
    {
        public const int NullStateId = 0;

        #region Singleton
        public static GameStateManager Instance { get; } = new GameStateManager();

        private GameStateManager() { }
        #endregion

        private readonly Dictionary<int, GameStateBase> stateMap = new Dictionary<int, GameStateBase>(11);

        private bool isPlaying;
        private bool isTransiting;
        private int startStateId;
        private GameStateBase currentState;

        public int CurrentStateId => currentState.Id;

        public void Initialize(GameStateConfig config)
        {
            Type attrType = typeof(ClassIdentityAttribute);
            for (int i = 0; i < config.gameStates.Length; ++i)
            {
                GameStateBase state = config.gameStates[i];
                if (Attribute.GetCustomAttribute(state.GetType(), attrType) is not ClassIdentityAttribute attr)
                {
                    Logging.Error($"There is no ClassIdentityAttribute on GameState. Class: {state}", nameof(GameStateManager));
                    continue;
                }

                if (stateMap.ContainsKey(attr.Id))
                {
                    Logging.Error($"Duplicated GameState Id: {attr.Id}", nameof(GameStateManager));
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
                Logging.Error($"GameStateManager has started.", nameof(GameStateManager));
                return;
            }

            CoroutineHelper.Instance.Start(StartRoutine());
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
                Logging.Error($"GameState is transiting. TargetState: {stateId}", nameof(GameStateManager));
                return;
            }

            CoroutineHelper.Instance.Start(TransitRoutine(stateId));
        }

        private IEnumerator TransitRoutine(int nextStateId)
        {
            isTransiting = true;
            currentState.enabled = false;
            yield return CoroutineHelper.WaitForEndOfFrame; // NOTE: Wait for the update of the current frame to complete.

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