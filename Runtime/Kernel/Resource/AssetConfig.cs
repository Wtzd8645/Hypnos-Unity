using System;
using UnityEngine;

namespace Morpheus.Resource
{
    [Serializable]
    public class AssetGroupData
    {
        public string groupId;
        public uint crc;
        public Hash128 hash;
        public string fileName;
        public ulong fileOffset;
        public string[] dependencies;
    }

    [Serializable]
    public class AssetData
    {
        public string assetId;
        public string assetPath;
        public string groupId;
    }

    [Serializable]
    public class AssetGroupManifest
    {
        public AssetGroupData[] assetGroupDatas;
    }

    [Serializable]
    public class AssetManifest
    {
        public AssetData[] assetDatas;
    }

    [Serializable]
    public class AssetConfig
    {
        public int version;
        public AssetGroupManifest assetGroupManifest;
        public AssetManifest assetManifest;
    }
}