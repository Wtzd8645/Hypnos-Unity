using System.Collections;
using UnityEngine;

namespace Morpheus.Resource
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
                Kernel.LogError($"[AssetGroupInst] AssetBundle is already loading asynchronously. GroupId: {groupId}");
                return;
            }

            bundle = AssetBundle.LoadFromFile(filePath, 0u, fileOffset);
            if (bundle == null)
            {
                Kernel.LogError($"[AssetGroupInst] Can't load AssetBundle from file. GroupId: {groupId}, FilePath: {filePath}, FileOffset: {fileOffset}");
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
                Kernel.LogError($"[AssetGroupInst] Can't load AssetBundle from file. GroupId: {groupId}, FilePath: {filePath}, FileOffset: {fileOffset}");
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
                Kernel.LogError($"[AssetGroupInst] Can't dereference when bundle is loading. GroupId: {groupId}");
            }

            if (--refCount > 0)
            {
                return false;
            }

            if (refCount < 0)
            {
                Kernel.LogError($"[AssetGroupInst] Reference count is abnormal. GroupId: {groupId}");
            }

            Unload();
            return true;
        }
    }
}