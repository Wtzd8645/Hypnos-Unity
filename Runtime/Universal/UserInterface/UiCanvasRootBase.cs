using System;
using UnityEngine;

namespace Hypnos.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public abstract class UiCanvasRootBase : MonoBehaviour
    {
        public abstract void RemoveAllUis();
        public abstract void OperateAsync(Type uiType, int op, UiAoHandler completeCb);
        public abstract void OperateAsync(int op, UiAoHandler completeCb);
    }
}