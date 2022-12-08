using UnityEngine;
using UnityEngine.UI;

namespace Morpheus.UI
{
    public class InverseMask : Mask
    {
        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return !isActiveAndEnabled || !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
        }
    }
}