using System;
using System.Reflection;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public static class UnityEngineUtil
    {
        public static Vector3 GetBodyPosition(Animator animator)
        {
            Type animatorType = Type.GetType("UnityEngine.Animator, UnityEngine");
            PropertyInfo bodyPosField = animatorType.GetProperty("bodyPositionInternal", BindingFlags.Instance | BindingFlags.NonPublic);
            return bodyPosField != null ? (Vector3)bodyPosField.GetValue(animator) : Vector3.zero;
        }

        public static void SetAllRendererEnabled(GameObject go, bool enabled)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = enabled;
            }
        }
    }
}