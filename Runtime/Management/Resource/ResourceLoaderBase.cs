﻿using System.Collections;

namespace Blanketmen.Hypnos
{
    internal abstract class ResourceLoaderBase
    {
        public abstract void SetAssetConfig(AssetRegistry config);

        public abstract bool IsBundleExists(string assetId);

        public abstract T LoadAsset<T>(string assetId) where T : UnityEngine.Object;

        public abstract IEnumerator LoadAssetRoutine<T>(string assetId, ResourceAoHandler<T> completeCb) where T : UnityEngine.Object;

        public abstract void UnloadAsset(string assetId);

        public abstract void UnloadAllAssets();
    }
}