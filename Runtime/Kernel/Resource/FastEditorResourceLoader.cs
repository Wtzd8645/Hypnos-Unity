#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Morpheus.Resource
{
    internal class FastEditorResourceLoader : ResourceLoaderBase
    {
        private Dictionary<string, AssetInst> assetInstMap;
        private Dictionary<string, string> assetPathMap;

        public override void SetAssetConfig(AssetConfig config)
        {
            AssetData[] assetDatas = config.assetManifest.assetDatas;
            assetInstMap = new Dictionary<string, AssetInst>(assetDatas.Length);
            assetPathMap = new Dictionary<string, string>(assetDatas.Length);
            foreach (AssetData data in assetDatas)
            {
                AssetInst assetInst = new AssetInst()
                {
                    assetId = data.assetId,
                    groupId = data.groupId,
                };
                assetInstMap.Add(data.assetId, assetInst);
                assetPathMap.Add(data.assetId, data.assetPath);
            }
        }

        public override bool IsBundleExists(string assetId)
        {
            return true;
        }

        public override T LoadAsset<T>(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Kernel.LogError($"[FastEditorResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                return null;
            }

            if (assetInst.asset == null)
            {
                assetInst.asset = AssetDatabase.LoadAssetAtPath<T>(assetPathMap[assetInst.assetId]);
            }

            ++assetInst.refCount;
            return assetInst.asset as T;
        }

        public override IEnumerator LoadAssetRoutine<T>(string assetId, ResourceAoHandler<T> completeCb)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Kernel.LogError($"[FastEditorResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                completeCb?.Invoke(null);
                yield break;
            }

            if (assetInst.asset == null)
            {
                assetInst.asset = AssetDatabase.LoadAssetAtPath<T>(assetPathMap[assetInst.assetId]);
                yield return null; // Simulate asynchronous loading
            }

            ++assetInst.refCount;
            completeCb?.Invoke(assetInst.asset as T);
        }

        public override void UnloadAsset(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Kernel.LogError($"[FastEditorResourceLoader] AssetInst is in an error state. AssetId: {assetId}");
                return;
            }

            assetInst.Dereference();
        }

        public override void UnloadAllAssets()
        {
            if (assetInstMap == null)
            {
                return;
            }

            foreach (AssetInst assetInst in assetInstMap.Values)
            {
                assetInst.asset = null;
                assetInst.refCount = 0;
            }
        }
    }
}
#endif