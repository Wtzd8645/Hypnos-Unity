using System;
using System.Collections.Generic;
using System.IO;

namespace Blanketmen.Hypnos
{
    public sealed class ArchiverManager
    {
        #region Singleton
        public static ArchiverManager Instance { get; } = new ArchiverManager();

        private ArchiverManager() { }
        #endregion

        public static string PersistentDataPath { get; private set; }
        public static string ArchiveFileExt { get; private set; }
        public static string ArchiveBackupFileExt { get; private set; }

        private readonly Dictionary<int, Archiver> archiverMap = new Dictionary<int, Archiver>(3);

        public void Initialize(ArchiveConfig config)
        {
            PersistentDataPath = config.persistentDataPath;

            ArchiveFileExt = config.archiveFileExt;
            ArchiveBackupFileExt = config.archiveBackupFileExt;
        }

        public void AddArchiver(int id, Archiver archiver)
        {
            archiverMap[id] = archiver;
        }

        public void RemoveArchiver(int id)
        {
            archiverMap.Remove(id);
        }

        public void SaveData<T>(int archiverId, T obj, string filePath)
        {
            archiverMap.TryGetValue(archiverId, out Archiver archiver);
            if (archiver == null)
            {
                Logging.Error($"DataArchiver is null. Id: {archiverId}", nameof(ResourceManager));
                return;
            }

            filePath = Path.Combine(PersistentDataPath, filePath);
            BackUpData(filePath);

            try
            {
                archiver.Save(obj, filePath + ArchiveFileExt);
            }
            catch (Exception e)
            {
                Logging.Error($"Save file failed. Exception: {e.Message}", nameof(ResourceManager));
                RestoreData(filePath);
            }
        }

        private void BackUpData(string filePath)
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
                Logging.Error($"Back up file failed. Exception: {e.Message}", nameof(ResourceManager));
            }
        }

        private void RestoreData(string filePath)
        {
            try
            {
                File.Copy(filePath + ArchiveBackupFileExt, filePath + ArchiveFileExt, true);
            }
            catch (Exception e)
            {
                Logging.Error($"Restore file failed. Exception: {e.Message}", nameof(ResourceManager));
            }
        }

        public T LoadData<T>(int archiverId, string filePath)
        {
            archiverMap.TryGetValue(archiverId, out Archiver archiver);
            if (archiver == null)
            {
                Logging.Error($"DataArchiver is null. Id: {archiverId}", nameof(ResourceManager));
                return default;
            }

            filePath = Path.Combine(PersistentDataPath, filePath);
            try
            {
                return archiver.Load<T>(filePath + ArchiveFileExt);
            }
            catch (Exception e)
            {
                Logging.Error($"Load file failed, use backup instead. Exception: {e.Message}", nameof(ResourceManager));
                return LoadBackupData<T>(archiver, filePath);
            }
        }

        private T LoadBackupData<T>(Archiver archiver, string filePath)
        {
            try
            {
                return archiver.Load<T>(filePath + ArchiveBackupFileExt);
            }
            catch (Exception e)
            {
                Logging.Error($"Load backup file failed. Exception: {e.Message}", nameof(ResourceManager));
                return default;
            }
        }
    }
}