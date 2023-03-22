using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor.Build
{
    internal abstract class AssetItemBase : TreeViewItem
    {
        protected const float IconSize = 14f;

        protected AssetTreeView tree;

        public abstract void OnGui(Rect cellRect, int column);
    }
}