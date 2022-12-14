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
            AssetData[] assetDatas = config.AssetManifest.AssetDatas;
            assetInstMap = new Dictionary<string, AssetInst>(assetDatas.Length);
            assetPathMap = new Dictionary<string, string>(assetDatas.Length);
            foreach (AssetData data in assetDatas)
            {
                AssetInst assetInst = new AssetInst()
                {
                    AssetId = data.AssetId,
                    GroupId = data.GroupId,
                };
                assetInstMap.Add(data.AssetId, assetInst);
                assetPathMap.Add(data.AssetId, data.AssetPath);
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
                DebugLogger.LogError($"[FastEditorResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                return null;
            }

            if (assetInst.Asset == null)
            {
                assetInst.Asset = AssetDatabase.LoadAssetAtPath<T>(assetPathMap[assetInst.AssetId]);
            }

            ++assetInst.RefCount;
            return assetInst.Asset as T;
        }

        public override IEnumerator LoadAssetRoutine<T>(string assetId, ResourceAoHandler<T> completeCb)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                DebugLogger.LogError($"[FastEditorResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                completeCb?.Invoke(null);
                yield break;
            }

            if (assetInst.Asset == null)
            {
                assetInst.Asset = AssetDatabase.LoadAssetAtPath<T>(assetPathMap[assetInst.AssetId]);
                yield return null; // Simulate asynchronous loading
            }

            ++assetInst.RefCount;
            completeCb?.Invoke(assetInst.Asset as T);
        }

        public override void UnloadAsset(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                DebugLogger.LogError($"[FastEditorResourceLoader] AssetInst is in an error state. AssetId: {assetId}");
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
                assetInst.Asset = null;
                assetInst.RefCount = 0;
            }
        }
    }
}
#endif