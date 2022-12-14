using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Morpheus.Resource
{
    internal class AssetBundleResourceLoader : ResourceLoaderBase
    {
        private readonly string resourcePath;
        private Dictionary<string, AssetGroupInst> assetGroupInstMap;
        private Dictionary<string, AssetInst> assetInstMap;

        public AssetBundleResourceLoader(string resourcePath)
        {
            this.resourcePath = resourcePath;
        }

        public override void SetAssetConfig(AssetConfig config)
        {
            AssetGroupData[] groupDatas = config.AssetGroupManifest.AssetGroupDatas;
            assetGroupInstMap = new Dictionary<string, AssetGroupInst>(groupDatas.Length);
            foreach (AssetGroupData data in groupDatas)
            {
                AssetGroupInst groupInst = new AssetGroupInst
                {
                    GroupId = data.GroupId,
                    FilePath = Path.Combine(resourcePath, data.FileName),
                    FileOffset = data.FileOffset,
                    Dependencies = data.Dependencies
                };
                assetGroupInstMap.Add(data.GroupId, groupInst);
            }

            AssetData[] assetDatas = config.AssetManifest.AssetDatas;
            assetInstMap = new Dictionary<string, AssetInst>(assetDatas.Length);
            foreach (AssetData data in assetDatas)
            {
                AssetInst assetInst = new AssetInst
                {
                    AssetId = data.AssetId,
                    GroupId = data.GroupId
                };
                assetInstMap.Add(data.AssetId, assetInst);
            }
        }

        public override bool IsBundleExists(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                return false;
            }

            assetGroupInstMap.TryGetValue(assetInst.GroupId, out AssetGroupInst groupInst);
            if (groupInst == null)
            {
                Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetGroupInst. GroupId: {assetInst.GroupId}, AssetId: {assetId}");
                return false;
            }

            return File.Exists(groupInst.FilePath);
        }

        public override T LoadAsset<T>(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                return null;
            }

            if (assetInst.Asset == null)
            {
                assetGroupInstMap.TryGetValue(assetInst.GroupId, out AssetGroupInst groupInst);
                if (groupInst == null)
                {
                    Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetGroupInst. GroupId: {assetInst.GroupId}, AssetId: {assetId}");
                    return null;
                }

                if (groupInst.Bundle == null && groupInst.RefCount == 0)
                {
                    // Load dependencies
                    foreach (string depGroupId in groupInst.Dependencies)
                    {
                        assetGroupInstMap.TryGetValue(depGroupId, out AssetGroupInst depGroupInst);
                        if (depGroupInst == null)
                        {
                            Kernel.LogError($"[AssetBundleResourceLoader] Can't find dependent AssetGroupInst. GroupId: {assetInst.GroupId}, DependenctGroupId: {depGroupId}");
                            continue;
                        }

                        ++depGroupInst.RefCount;
                        depGroupInst.Load();
                    }

                    ++groupInst.RefCount;
                    groupInst.Load();
                }

                groupInst.LoadAsset<T>(assetInst);
            }

            ++assetInst.RefCount;
            return assetInst.Asset as T;
        }

        public override IEnumerator LoadAssetRoutine<T>(string assetId, ResourceAoHandler<T> completeCb)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                completeCb?.Invoke(null);
                yield break;
            }

            if (assetInst.Asset == null)
            {
                assetGroupInstMap.TryGetValue(assetInst.GroupId, out AssetGroupInst groupInst);
                if (groupInst == null)
                {
                    Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetGroupInst. GroupId: {assetInst.GroupId}, AssetId: {assetId}");
                    yield break;
                }

                if (groupInst.Bundle == null && groupInst.RefCount == 0)
                {
                    // Load dependencies
                    foreach (string depGroupId in groupInst.Dependencies)
                    {
                        assetGroupInstMap.TryGetValue(depGroupId, out AssetGroupInst depGroupInst);
                        if (depGroupInst == null)
                        {
                            Kernel.LogError($"[AssetBundleResourceLoader] Can't find dependent AssetGroupInst. GroupId: {assetInst.GroupId}, DependenctGroupId: {depGroupId}");
                            continue;
                        }

                        ++depGroupInst.RefCount;
                        yield return depGroupInst.LoadRoutine();
                    }

                    ++groupInst.RefCount;
                    yield return groupInst.LoadRoutine();
                }

                yield return groupInst.LoadAssetRoutine<T>(assetInst);
            }

            ++assetInst.RefCount;
            completeCb?.Invoke(assetInst.Asset as T);
        }

        public override void UnloadAsset(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetInst. AssetId: {assetId}");
                return;
            }

            if (!assetInst.Dereference())
            {
                return;
            }

            DereferenceAssetGroupInst(assetInst.GroupId);
        }

        private void DereferenceAssetGroupInst(string groupId)
        {
            assetGroupInstMap.TryGetValue(groupId, out AssetGroupInst groupInst);
            if (groupInst == null)
            {
                Kernel.LogError($"[AssetBundleResourceLoader] Can't find AssetGroupInst. GroupId: {groupId}");
                return;
            }

            if (!groupInst.Dereference())
            {
                return;
            }

            // Unload dependencies
            foreach (string depGroupId in groupInst.Dependencies)
            {
                assetGroupInstMap.TryGetValue(depGroupId, out AssetGroupInst depGroupInst);
                if (depGroupInst == null)
                {
                    Kernel.LogError($"[AssetBundleResourceLoader] Can't find dependent AssetGroupInst. GroupId: {groupId}, DependenctGroupId: {depGroupId}");
                    continue;
                }
                depGroupInst.Dereference();
            }
        }

        public override void UnloadAllAssets()
        {
            if (assetInstMap == null || assetGroupInstMap == null)
            {
                return;
            }

            foreach (AssetInst assetInst in assetInstMap.Values)
            {
                assetInst.Asset = null;
                assetInst.RefCount = 0;
            }

            foreach (AssetGroupInst groupInst in assetGroupInstMap.Values)
            {
                groupInst.Unload();
            }
        }
    }
}