using System.Collections;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    internal class AssetGroupInst
    {
        public string groupId;
        public string filePath;
        public ulong fileOffset;
        public string[] dependencies;

        public AssetBundle bundle;
        public bool isLoading;
        public int refCount;

        public void Load()
        {
            if (bundle != null)
            {
                return;
            }

            if (isLoading)
            {
                Logging.Error($"The assetBundle is already loading asynchronously. GroupId: {groupId}", nameof(AssetGroupInst));
                return;
            }

            bundle = AssetBundle.LoadFromFile(filePath, 0u, fileOffset);
            if (bundle == null)
            {
                Logging.Error($"Can't load AssetBundle from file. GroupId: {groupId}, FilePath: {filePath}, FileOffset: {fileOffset}", nameof(AssetGroupInst));
            }
        }

        public IEnumerator LoadRoutine()
        {
            if (bundle != null)
            {
                yield break;
            }

            while (isLoading)
            {
                yield return null;
            }

            isLoading = true;
            AssetBundleCreateRequest bundleLoadAo = AssetBundle.LoadFromFileAsync(filePath, 0u, fileOffset);
            yield return bundleLoadAo;

            isLoading = false;
            bundle = bundleLoadAo.assetBundle;
            if (bundle == null)
            {
                Logging.Error($"Can't load AssetBundle from file. GroupId: {groupId}, FilePath: {filePath}, FileOffset: {fileOffset}", nameof(AssetGroupInst));
            }
        }

        public void Unload()
        {
            if (bundle != null)
            {
                bundle.Unload(true);
                bundle = null;
            }
            refCount = 0;
        }

        public void LoadAsset<T>(AssetInst assetInst) where T : UnityEngine.Object
        {
            if (bundle == null)
            {
                return;
            }

            assetInst.asset = bundle.LoadAsset<T>(assetInst.assetId);
        }

        public IEnumerator LoadAssetRoutine<T>(AssetInst assetInst) where T : UnityEngine.Object
        {
            if (bundle == null)
            {
                yield break;
            }

            AssetBundleRequest assetLoadAo = bundle.LoadAssetAsync(assetInst.assetId);
            yield return assetLoadAo;

            assetInst.asset = assetLoadAo.asset;
        }

        public bool Dereference()
        {
            if (isLoading)
            {
                Logging.Error($"Can't dereference when bundle is loading. GroupId: {groupId}", nameof(AssetGroupInst));
            }

            if (--refCount > 0)
            {
                return false;
            }

            if (refCount < 0)
            {
                Logging.Error($"Reference count is abnormal. GroupId: {groupId}", nameof(AssetGroupInst));
            }

            Unload();
            return true;
        }
    }
}