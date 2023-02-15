using UnityEngine;
using UnityEngine.UI;

namespace Hypnos.UI
{
    public class InverseMask : Mask
    {
        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return !isActiveAndEnabled || !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
        }
    }
}