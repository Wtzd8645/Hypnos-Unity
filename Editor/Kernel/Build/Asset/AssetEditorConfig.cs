using System;
using System.Collections.Generic;
using UnityEngine;

namespace Morpheus.Editor.Build
{
    public enum AssetType
    {
        Directory,
        Asset
    }

    [Serializable]
    public class AssetEditorData
    {
        public AssetType assetType;
        public string assetId;
        public string assetPath;
        public List<AssetEditorData> subAssets;
    }

    [Serializable]
    public class AssetGroupEditorData
    {
        public string groupId;
        public CompressionType compression;
        public List<AssetEditorData> assetDatas;
    }

    [Serializable]
    public class AssetEditorConfig
    {
        public int version;
        public List<AssetGroupEditorData> groupDatas = new List<AssetGroupEditorData>(32);
    }
}