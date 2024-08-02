using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blanketmen.Hypnos.Editor.Build
{
    internal class AssetTreeItem : AssetItemBase
    {
        private Object asset;

        public override string displayName { get => Data.assetId; set => Data.assetId = value; }
        public string AssetPath { get => Data.assetPath; }
        public AssetEditorData Data { get; private set; }

        public AssetTreeItem(AssetEditorData data, AssetTreeView ownerTree, int treeDepth)
        {
            id = ownerTree.FetchItemId();
            depth = treeDepth++;
            tree = ownerTree;
            asset = AssetDatabase.LoadAssetAtPath<Object>(data.assetPath);
            if (asset != null)
            {
                icon = AssetDatabase.GetCachedIcon(data.assetPath) as Texture2D;
            }

            Data = data;
        }

        public AssetTreeItem(string assetId, string assetPath, AssetTreeView ownerTree, int treeDepth)
        {
            id = ownerTree.FetchItemId();
            depth = treeDepth;
            tree = ownerTree;
            asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
            {
                icon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D;
            }

            Data = new AssetEditorData
            {
                assetId = assetId,
                assetPath = assetPath,
                assetType = Directory.Exists(assetPath) ? AssetType.Directory : AssetType.Asset
            };
        }

        public AssetEditorData FetchData()
        {
            if (!hasChildren)
            {
                Data.subAssets = null;
                return Data;
            }

            List<AssetEditorData> assetDatas = new List<AssetEditorData>(children.Count);
            foreach (TreeViewItem child in children)
            {
                assetDatas.Add((child as AssetTreeItem).FetchData());
            }
            Data.subAssets = assetDatas;
            return Data;
        }

        public override void OnGui(Rect cellRect, int column)
        {
            switch (column)
            {
                case 0:
                {
                    cellRect.xMin += (depth + 1) * IconSize;
                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y + 1f, IconSize, IconSize), icon, ScaleMode.ScaleToFit);

                    cellRect.xMin += IconSize;
                    EditorGUI.LabelField(cellRect, Data.assetId);
                    break;
                }
                case 1:
                {
                    GUI.Label(cellRect, $"{Data.assetPath}");
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