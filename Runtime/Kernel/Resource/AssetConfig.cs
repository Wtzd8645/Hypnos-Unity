using System;
using UnityEngine;

namespace Morpheus.Resource
{
    [Serializable]
    public class AssetGroupData
    {
        public string GroupId;
        public uint Crc;
        public Hash128 Hash;
        public string FileName;
        public ulong FileOffset;
        public string[] Dependencies;
    }

    [Serializable]
    public class AssetData
    {
        public string AssetId;
        public string AssetPath;
        public string GroupId;
    }

    [Serializable]
    public class AssetGroupManifest
    {
        public AssetGroupData[] AssetGroupDatas;
    }

    [Serializable]
    public class AssetManifest
    {
        public AssetData[] AssetDatas;
    }

    [Serializable]
    public class AssetConfig
    {
        public int Version;
        public AssetGroupManifest AssetGroupManifest;
        public AssetManifest AssetManifest;
    }
}