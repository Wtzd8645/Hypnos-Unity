using System.Collections;
using UnityEngine;

namespace Morpheus.Resource
{
    internal class AssetGroupInst
    {
        public string GroupId;
        public string FilePath;
        public ulong FileOffset;
        public string[] Dependencies;

        public AssetBundle Bundle;
        public bool IsLoading;
        public int RefCount;

        public void Load()
        {
            if (Bundle != null)
            {
                return;
            }

            if (IsLoading)
            {
                Logger.LogError($"[AssetGroupInst] AssetBundle is already loading asynchronously. GroupId: {GroupId}");
                return;
            }

            Bundle = AssetBundle.LoadFromFile(FilePath, 0u, FileOffset);
            if (Bundle == null)
            {
                Logger.LogError($"[AssetGroupInst] Can't load AssetBundle from file. GroupId: {GroupId}, FilePath: {FilePath}, FileOffset: {FileOffset}");
            }
        }

        public IEnumerator LoadRoutine()
        {
            if (Bundle != null)
            {
                yield break;
            }

            while (IsLoading)
            {
                yield return null;
            }

            IsLoading = true;
            AssetBundleCreateRequest bundleLoadAo = AssetBundle.LoadFromFileAsync(FilePath, 0u, FileOffset);
            yield return bundleLoadAo;

            IsLoading = false;
            Bundle = bundleLoadAo.assetBundle;
            if (Bundle == null)
            {
                Logger.LogError($"[AssetGroupInst] Can't load AssetBundle from file. GroupId: {GroupId}, FilePath: {FilePath}, FileOffset: {FileOffset}");
            }
        }

        public void Unload()
        {
            if (Bundle != null)
            {
                Bundle.Unload(true);
                Bundle = null;
            }
            RefCount = 0;
        }

        public void LoadAsset<T>(AssetInst assetInst) where T : UnityEngine.Object
        {
            if (Bundle == null)
            {
                return;
            }

            assetInst.Asset = Bundle.LoadAsset<T>(assetInst.AssetId);
        }

        public IEnumerator LoadAssetRoutine<T>(AssetInst assetInst) where T : UnityEngine.Object
        {
            if (Bundle == null)
            {
                yield break;
            }

            AssetBundleRequest assetLoadAo = Bundle.LoadAssetAsync(assetInst.AssetId);
            yield return assetLoadAo;

            assetInst.Asset = assetLoadAo.asset;
        }

        public bool Dereference()
        {
            if (IsLoading)
            {
                Logger.LogError($"[AssetGroupInst] Can't dereference when bundle is loading. GroupId: {GroupId}");
            }

            if (--RefCount > 0)
            {
                return false;
            }

            if (RefCount < 0)
            {
                Logger.LogError($"[AssetGroupInst] Reference count is abnormal. GroupId: {GroupId}");
            }

            Unload();
            return true;
        }
    }
}