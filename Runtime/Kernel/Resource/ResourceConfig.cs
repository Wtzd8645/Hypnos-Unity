using UnityEngine;

namespace Morpheus.Resource
{
    public class ResourceConfig : ScriptableObject
    {
        public string AppDataPath;
        public string PersistentDataPath;
        public string StreamingDataPath;

        public string ArchiveFileExt = ".dat";
        public string ArchiveBackupFileExt = ".bak";
        public DataArchiverConfig[] DataArchiverConfigs;

        public ResourceLoader ResourceLoader;
    }
}