using Blanketmen.Hypnos.Serialization;
using System;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public delegate void ResourceAoHandler<T>(T obj);

    public enum ResourceLoader
    {
        FastEditor, // Development
        BuildInAssetBundle, // Testing
        AssetBundle // Staging & Production
    }

    public sealed partial class ResourceManager
    {
        public const string ResourcesDirectoryName = "Resources";
        public const string AssetConfigFileName = "AssetConfig.dat";

        public static readonly Archiver AssetConfigArchiver = new Archiver(new DotNetXmlSerializer());

        #region Singleton
        public static ResourceManager Instance { get; } = new ResourceManager();

        private ResourceManager() { }
        #endregion

        private ResourceLoaderBase resourceLoader;

        public static string ArchiveFileExt { get; private set; }
        public static string ArchiveBackupFileExt { get; private set; }

        public static string AppDataPath { get; private set; }
        public static string PersistentDataPath { get; private set; }
        public static string StreamingDataPath { get; private set; }

        public string ResourcesDirectoryPath { get; private set; }

        public void Initialize(ResourceConfig config)
        {
            AppDataPath = config.appDataPath;
            StreamingDataPath = config.streamingDataPath;

            switch (config.resourceLoader)
            {
#if UNITY_EDITOR
                case ResourceLoader.FastEditor:
                {
                    //ResourcesDirectoryPath = Path.Combine(config.persistentDataPath, ResourcesDirectoryName);
                    //AssetConfigPath = Path.Combine(ResourcesDirectoryPath, AssetConfigFileName);
                    resourceLoader = new FastEditorResourceLoader();
                    break;
                }
#endif
                // NOTE: StreamingDataPath is readonly.
                case ResourceLoader.BuildInAssetBundle:
                {
                    //ResourcesDirectoryPath = Path.Combine(config.streamingDataPath, ResourcesDirectoryName);
                    //AssetConfigPath = Path.Combine(ResourcesDirectoryPath, AssetConfigFileName);
                    resourceLoader = new AssetBundleResourceLoader(ResourcesDirectoryPath);
                    break;
                }
                case ResourceLoader.AssetBundle:
                {
                    //ResourcesDirectoryPath = Path.Combine(config.persistentDataPath, ResourcesDirectoryName);
                    //AssetConfigPath = Path.Combine(ResourcesDirectoryPath, AssetConfigFileName);
                    resourceLoader = new AssetBundleResourceLoader(ResourcesDirectoryPath);
                    break;
                }
                default:
                {
                    Logging.Error($"The ResourceLoader type is not supported. {config.resourceLoader}", nameof(ResourceManager));
                    break;
                }
            }
        }

        public void Release()
        {
            resourceLoader.UnloadAllAssets();
        }

        public void SetAssetRegistry(AssetRegistry registry)
        {
            try
            {
                resourceLoader.SetAssetConfig(registry); // TODO: Handle re-entry situation.
            }
            catch (Exception e)
            {
                Logging.Error($"Can't load AssetConfig. Exception: {e.Message}", nameof(ResourceManager));
            }
        }

        public bool IsBundleExists(string assetId) => resourceLoader.IsBundleExists(assetId);

        public T LoadAsset<T>(string assetId) where T : UnityEngine.Object
        {
            return resourceLoader.LoadAsset<T>(assetId);
        }

        public void LoadAssetAsync<T>(string assetId, ResourceAoHandler<T> completeCb = null) where T : UnityEngine.Object
        {
            CoroutineHelper.Instance.Start(resourceLoader.LoadAssetRoutine(assetId, completeCb));
        }

        public void UnloadAsset(string assetId)
        {
            resourceLoader.UnloadAsset(assetId);
        }

        public void UnloadAllAsset()
        {
            resourceLoader.UnloadAllAssets();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public T Create<T>(T prefab, Transform parent = null, bool isWorldPositionStays = false) where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(prefab, parent, isWorldPositionStays);
        }

        public void Destroy(UnityEngine.Object obj)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }
}