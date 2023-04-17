using System.Collections;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public static class Coroutiner
    {
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        private static MonoBehaviour coroutineExecutor = null;

        // TODO: Temporarily use a single MonoBehaviour to execute coroutines,
        // but it may affect performance by executing too many coroutines in one frame.
        public static void Initialize(MonoBehaviour executor)
        {
            if (coroutineExecutor == null)
            {
                coroutineExecutor = executor;
            }
        }

        public static Coroutine Execute(IEnumerator routine)
        {
            return coroutineExecutor.StartCoroutine(routine);
        }

        public static void Terminate(Coroutine routine)
        {
            coroutineExecutor.StopCoroutine(routine);
        }

        public static void TerminateAllCoroutines()
        {
            coroutineExecutor.StopAllCoroutines();
        }
    }
}