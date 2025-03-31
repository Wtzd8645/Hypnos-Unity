using System.Collections;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public abstract class GameStateBase : MonoBehaviour
    {
        public int Id { get; internal set; }

        private void Reset() { enabled = false; }

        private void Awake() { enabled = false; }

        // NOTE: Do not write suspend function in OnEnable() / OnDisable().
        // NOTE: The current update will still be completed, even if called in any Unity update.
        // WARNING: Pay attention to the timing when using asynchronous functions in the coroutine.
        protected internal virtual IEnumerator OnEnter(int prevState) { yield break; }
        protected internal virtual void OnTransitionComplete() { }
        protected internal virtual IEnumerator OnExit(int nextState) { yield break; }
    }
}