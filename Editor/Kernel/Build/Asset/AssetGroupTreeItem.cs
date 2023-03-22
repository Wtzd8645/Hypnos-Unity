using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor.Build
{
    internal class AssetGroupTreeItem : AssetItemBase
    {
        public override string displayName { get => Data.groupId; set => Data.groupId = value; }
        public AssetGroupEditorData Data { get; private set; }

        public AssetGroupTreeItem(AssetGroupEditorData data, AssetTreeView ownerTree, int treeDepth)
        {
            id = ownerTree.FetchItemId();
            tree = ownerTree;
            depth = treeDepth++;

            Data = data;
        }

        public AssetGroupTreeItem(string name, AssetTreeView ownerTree, int treeDepth)
        {
            id = ownerTree.FetchItemId();
            tree = ownerTree;
            depth = treeDepth;

            Data = new AssetGroupEditorData
            {
                groupId = name
            };
        }

        public AssetGroupEditorData FetchData()
        {
            if (!hasChildren)
            {
                Data.assetDatas = null;
                return Data;
            }

            List<AssetEditorData> assetDatas = new List<AssetEditorData>(children.Count);
            foreach (TreeViewItem child in children)
            {
                assetDatas.Add((child as AssetTreeItem).FetchData());
            }
            Data.assetDatas = assetDatas;
            return Data;
        }

        public override void OnGui(Rect cellRect, int column)
        {
            switch (column)
            {
                case 0:
                {
                    cellRect.xMin += (depth + 1) * IconSize;
                    GUI.Label(cellRect, $"{Data.groupId}");
                    break;
                }
                case 2:
                {
                    break;
                }
            }
        }
    }
}