using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Morpheus.Resource
{
    public sealed partial class ResourceManager
    {
        #region Singleton
        public static ResourceManager Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance ??= new ResourceManager();
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }
        #endregion

        private Dictionary<int, DataArchiver> dataArchiverMap;
        private ResourceLoaderBase resourceLoader;

        public static string ArchiveFileExt { get; private set; }
        public static string ArchiveBackupFileExt { get; private set; }

        public static string AppDataPath { get; private set; }
        public static string PersistentDataPath { get; private set; }
        public static string StreamingDataPath { get; private set; }

        public string ResourcesDirectoryPath { get; private set; }
        public string AssetConfigPath { get; private set; }

        private ResourceManager() { }

        public void Initialize(ResourceConfig config)
        {
            ArchiveFileExt = config.archiveFileExt;
            ArchiveBackupFileExt = config.archiveBackupFileExt;
            AppDataPath = config.appDataPath;
            PersistentDataPath = config.persistentDataPath;
            StreamingDataPath = config.streamingDataPath;

            dataArchiverMap = new Dictionary<int, DataArchiver>(config.dataArchiverConfigs.Length);
            foreach (DataArchiverConfig archiverConfig in config.dataArchiverConfigs)
            {
                DataArchiver archiver = new DataArchiver(
                    CoreUtil.CreateSerializer(archiverConfig.serializer),
                    CoreUtil.CreateCompressor(archiverConfig.compressor),
                    CoreUtil.CreateEncryptor(archiverConfig.encryptor));
                dataArchiverMap.Add(archiverConfig.id, archiver);
            }

            switch (config.resourceLoader)
            {
#if UNITY_EDITOR
                case ResourceLoader.FastEditor:
                {
                    ResourcesDirectoryPath = Path.Combine(config.persistentDataPath, ResourcesDirectoryName);
                    AssetConfigPath = Path.Combine(ResourcesDirectoryPath, AssetConfigFileName);
                    resourceLoader = new FastEditorResourceLoader();
                    break;
                }
#endif
                // NOTE: StreamingDataPath is readonly.
                case ResourceLoader.BuildInAssetBundle:
                {
                    ResourcesDirectoryPath = Path.Combine(config.streamingDataPath, ResourcesDirectoryName);
                    AssetConfigPath = Path.Combine(ResourcesDirectoryPath, AssetConfigFileName);
                    resourceLoader = new AssetBundleResourceLoader(ResourcesDirectoryPath);
                    break;
                }
                case ResourceLoader.AssetBundle:
                {
                    ResourcesDirectoryPath = Path.Combine(config.persistentDataPath, ResourcesDirectoryName);
                    AssetConfigPath = Path.Combine(ResourcesDirectoryPath, AssetConfigFileName);
                    resourceLoader = new AssetBundleResourceLoader(ResourcesDirectoryPath);
                    break;
                }
                default:
                {
                    DebugLogger.LogError($"[ResourceManager] The ResourceLoader type is not supported. {config.resourceLoader}");
                    break;
                }
            }
        }

        public void Release()
        {
            resourceLoader.UnloadAllAssets();
        }

        #region Archive
        public void SaveArchive<T>(int archiverId, T obj, string filePath)
        {
            dataArchiverMap.TryGetValue(archiverId, out DataArchiver archiver);
            if (archiver == null)
            {
                DebugLogger.LogError($"[ResourceManager] DataArchiver is null. Id: {archiverId}");
                return;
            }

            filePath = Path.Combine(PersistentDataPath, filePath);
            BackUpArchive(filePath);

            try
            {
                archiver.Save(obj, filePath + ArchiveFileExt);
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[ResourceManager] Save file failed. Exception: {e.Message}");
                RestoreArchive(filePath);
            }
        }

        private void BackUpArchive(string filePath)
        {
            try
            {
                File.Copy(filePath + ArchiveFileExt, filePath + ArchiveBackupFileExt, true);
            }
            catch (FileNotFoundException)
            {
                // NOTE: No backup for the first save.
                return;
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[ResourceManager] Back up file failed. Exception: {e.Message}");
            }
        }

        private void RestoreArchive(string filePath)
        {
            try
            {
                File.Copy(filePath + ArchiveBackupFileExt, filePath + ArchiveFileExt, true);
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[ResourceManager] Restore file failed. Exception: {e.Message}");
            }
        }

        public T LoadArchive<T>(int archiverId, string filePath)
        {
            dataArchiverMap.TryGetValue(archiverId, out DataArchiver archiver);
            if (archiver == null)
            {
                DebugLogger.LogError($"[ResourceManager] DataArchiver is null. Id: {archiverId}");
                return default;
            }

            filePath = Path.Combine(PersistentDataPath, filePath);
            try
            {
                return archiver.Load<T>(filePath + ArchiveFileExt);
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[ResourceManager] Load file failed, use backup instead. Exception: {e.Message}");
                return LoadBackupArchive<T>(archiver, filePath);
            }
        }

        private T LoadBackupArchive<T>(DataArchiver archiver, string filePath)
        {
            try
            {
                return archiver.Load<T>(filePath + ArchiveBackupFileExt);
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[ResourceManager] Load backup file failed. Exception: {e.Message}");
                return default;
            }
        }
        #endregion

        public void UpdateAssetConfig()
        {
            try
            {
                DataArchiver archiver = new DataArchiver(
                   CoreUtil.CreateSerializer(AssetConfigSerializer),
                   CoreUtil.CreateCompressor(AssetConfigCompressor),
                   CoreUtil.CreateEncryptor(AssetConfigEncryptor));
                AssetConfig config = archiver.Load<AssetConfig>(AssetConfigPath);
                resourceLoader.SetAssetConfig(config); // TODO: Handle re-entry situation.
            }
            catch (Exception e)
            {
                DebugLogger.LogError($"[ResourceManager] Can't load AssetConfig. Exception: {e.Message}");
            }
        }

        public bool IsBundleExists(string assetId)
        {
            return resourceLoader.IsBundleExists(assetId);
        }

        public T LoadAsset<T>(string assetId) where T : UnityEngine.Object
        {
            return resourceLoader.LoadAsset<T>(assetId);
        }

        public void LoadAssetAsync<T>(string assetId, ResourceAoHandler<T> completeCb = null) where T : UnityEngine.Object
        {
            AppKernel.ExecuteCoroutine(resourceLoader.LoadAssetRoutine(assetId, completeCb));
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