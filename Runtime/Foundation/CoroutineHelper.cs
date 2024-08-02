using System.Collections;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class CoroutineHelper
    {
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        #region Singleton
        public static CoroutineHelper Instance { get; } = new CoroutineHelper();

        private CoroutineHelper() { }
        #endregion

        private MonoBehaviour runner;

        // TODO: Temporarily use a single MonoBehaviour to execute coroutines,
        // but it may affect performance by executing too many coroutines in one frame.
        public void Initialize(MonoBehaviour runner)
        {
            this.runner = runner;
        }

        public Coroutine Start(IEnumerator routine)
        {
            return runner.StartCoroutine(routine);
        }

        public void Stop(Coroutine routine)
        {
            runner.StopCoroutine(routine);
        }

        public void StopAll()
        {
            runner.StopAllCoroutines();
        }
    }
}