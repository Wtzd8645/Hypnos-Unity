using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Hypnos.Editor.Build
{
    internal class EditionItem : TreeViewItem
    {
        public EditionConfig Data { get; private set; }

        public EditionItem(EditionConfig data, EditionTreeView ownerTree)
        {
            id = ownerTree.FetchItemId();
            displayName = data.name;
            Data = data;
        }

        public void Rename(string name)
        {
            displayName = name;
            Data.name = name;
        }

        public void OnGui(Rect cellRect)
        {
            cellRect.x += 16f;
            GUI.Label(cellRect, $"{displayName}");
        }
    }
}