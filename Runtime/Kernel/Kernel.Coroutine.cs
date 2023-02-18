using System.Collections;
using UnityEngine;

namespace Hypnos
{
    public partial class Kernel
    {
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        public static Coroutine Execute(IEnumerator routine)
        {
            return instance.StartCoroutine(routine);
        }

        public static void Terminate(Coroutine routine)
        {
            instance.StopCoroutine(routine);
        }

        public static void TerminateAllCoroutines()
        {
            instance.StopAllCoroutines();
        }
    }
}