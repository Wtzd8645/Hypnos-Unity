using System.Collections;

namespace Hypnos.Resource
{
    internal abstract class ResourceLoaderBase
    {
        public abstract void SetAssetConfig(AssetConfig config);

        public abstract bool IsBundleExists(string assetId);

        public abstract T LoadAsset<T>(string assetId) where T : UnityEngine.Object;

        public abstract IEnumerator LoadAssetRoutine<T>(string assetId, ResourceAoHandler<T> completeCb) where T : UnityEngine.Object;

        public abstract void UnloadAsset(string assetId);

        public abstract void UnloadAllAssets();
    }
}