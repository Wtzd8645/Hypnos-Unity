using System;
using System.Collections;
using UnityEngine;

namespace Morpheus
{
    [Flags]
    public enum RunningVersion
    {
        Release = 0,
        Stage = 1,
        Development = 2,
        Test = 3
    }

    public static partial class Kernel
    {
#if TEST
        public const RunningVersion Version = RunningVersion.Test;
#elif DEVELOPMENT
        public const RunningVersion Version = RunningVersion.Development;
#elif STAGE
        public const RunningVersion Version = RunningVersion.Stage;
#else
        public const RunningVersion Version = RunningVersion.Release;
#endif

        #region Coroutine
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        private static MonoBehaviour coroutineExecutor;

        public static void SetCoroutineExecutor(MonoBehaviour executor)
        {
            coroutineExecutor = executor;
        }

        public static Coroutine ExecuteCoroutine(IEnumerator routine)
        {
            return coroutineExecutor.StartCoroutine(routine);
        }

        public static void TerminateCoroutine(Coroutine routine)
        {
            coroutineExecutor.StopCoroutine(routine);
        }

        public static void TerminateAllCoroutines()
        {
            coroutineExecutor.StopAllCoroutines();
        }
        #endregion
    }
}