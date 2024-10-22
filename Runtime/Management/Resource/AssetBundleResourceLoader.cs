using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Blanketmen.Hypnos
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

        public override void SetAssetConfig(AssetRegistry config)
        {
            AssetGroupData[] groupDatas = config.assetGroupManifest.assetGroupDatas;
            assetGroupInstMap = new Dictionary<string, AssetGroupInst>(groupDatas.Length);
            foreach (AssetGroupData data in groupDatas)
            {
                AssetGroupInst groupInst = new AssetGroupInst
                {
                    groupId = data.groupId,
                    filePath = Path.Combine(resourcePath, data.fileName),
                    fileOffset = data.fileOffset,
                    dependencies = data.dependencies
                };
                assetGroupInstMap.Add(data.groupId, groupInst);
            }

            AssetData[] assetDatas = config.assetManifest.assetDatas;
            assetInstMap = new Dictionary<string, AssetInst>(assetDatas.Length);
            foreach (AssetData data in assetDatas)
            {
                AssetInst assetInst = new AssetInst
                {
                    assetId = data.assetId,
                    groupId = data.groupId
                };
                assetInstMap.Add(data.assetId, assetInst);
            }
        }

        public override bool IsBundleExists(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Logging.Error($"Can't find AssetInst. AssetId: {assetId}", nameof(AssetBundleResourceLoader));
                return false;
            }

            assetGroupInstMap.TryGetValue(assetInst.groupId, out AssetGroupInst groupInst);
            if (groupInst == null)
            {
                Logging.Error($"Can't find AssetGroupInst. GroupId: {assetInst.groupId}, AssetId: {assetId}", nameof(AssetBundleResourceLoader));
                return false;
            }

            return File.Exists(groupInst.filePath);
        }

        public override T LoadAsset<T>(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Logging.Error($"Can't find AssetInst. AssetId: {assetId}", nameof(AssetBundleResourceLoader));
                return null;
            }

            if (assetInst.asset == null)
            {
                assetGroupInstMap.TryGetValue(assetInst.groupId, out AssetGroupInst groupInst);
                if (groupInst == null)
                {
                    Logging.Error($"Can't find AssetGroupInst. GroupId: {assetInst.groupId}, AssetId: {assetId}", nameof(AssetBundleResourceLoader));
                    return null;
                }

                if (groupInst.bundle == null && groupInst.refCount == 0)
                {
                    // Load dependencies
                    foreach (string depGroupId in groupInst.dependencies)
                    {
                        assetGroupInstMap.TryGetValue(depGroupId, out AssetGroupInst depGroupInst);
                        if (depGroupInst == null)
                        {
                            Logging.Error($"Can't find dependent AssetGroupInst. GroupId: {assetInst.groupId}, DependenctGroupId: {depGroupId}", nameof(AssetBundleResourceLoader));
                            continue;
                        }

                        ++depGroupInst.refCount;
                        depGroupInst.Load();
                    }

                    ++groupInst.refCount;
                    groupInst.Load();
                }

                groupInst.LoadAsset<T>(assetInst);
            }

            ++assetInst.refCount;
            return assetInst.asset as T;
        }

        public override IEnumerator LoadAssetRoutine<T>(string assetId, ResourceAoHandler<T> completeCb)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Logging.Error($"Can't find AssetInst. AssetId: {assetId}", nameof(AssetBundleResourceLoader));
                completeCb?.Invoke(null);
                yield break;
            }

            if (assetInst.asset == null)
            {
                assetGroupInstMap.TryGetValue(assetInst.groupId, out AssetGroupInst groupInst);
                if (groupInst == null)
                {
                    Logging.Error($"Can't find AssetGroupInst. GroupId: {assetInst.groupId}, AssetId: {assetId}", nameof(AssetBundleResourceLoader));
                    yield break;
                }

                if (groupInst.bundle == null && groupInst.refCount == 0)
                {
                    // Load dependencies
                    foreach (string depGroupId in groupInst.dependencies)
                    {
                        assetGroupInstMap.TryGetValue(depGroupId, out AssetGroupInst depGroupInst);
                        if (depGroupInst == null)
                        {
                            Logging.Error($"Can't find dependent AssetGroupInst. GroupId: {assetInst.groupId}, DependenctGroupId: {depGroupId}", nameof(AssetBundleResourceLoader));
                            continue;
                        }

                        ++depGroupInst.refCount;
                        yield return depGroupInst.LoadRoutine();
                    }

                    ++groupInst.refCount;
                    yield return groupInst.LoadRoutine();
                }

                yield return groupInst.LoadAssetRoutine<T>(assetInst);
            }

            ++assetInst.refCount;
            completeCb?.Invoke(assetInst.asset as T);
        }

        public override void UnloadAsset(string assetId)
        {
            assetInstMap.TryGetValue(assetId, out AssetInst assetInst);
            if (assetInst == null)
            {
                Logging.Error($"Can't find AssetInst. AssetId: {assetId}", nameof(AssetBundleResourceLoader));
                return;
            }

            if (!assetInst.Dereference())
            {
                return;
            }

            DereferenceAssetGroupInst(assetInst.groupId);
        }

        private void DereferenceAssetGroupInst(string groupId)
        {
            assetGroupInstMap.TryGetValue(groupId, out AssetGroupInst groupInst);
            if (groupInst == null)
            {
                Logging.Error($"Can't find AssetGroupInst. GroupId: {groupId}", nameof(AssetBundleResourceLoader));
                return;
            }

            if (!groupInst.Dereference())
            {
                return;
            }

            // Unload dependencies
            foreach (string depGroupId in groupInst.dependencies)
            {
                assetGroupInstMap.TryGetValue(depGroupId, out AssetGroupInst depGroupInst);
                if (depGroupInst == null)
                {
                    Logging.Error($"Can't find dependent AssetGroupInst. GroupId: {groupId}, DependenctGroupId: {depGroupId}", nameof(AssetBundleResourceLoader));
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
                assetInst.asset = null;
                assetInst.refCount = 0;
            }

            foreach (AssetGroupInst groupInst in assetGroupInstMap.Values)
            {
                groupInst.Unload();
            }
        }
    }
}