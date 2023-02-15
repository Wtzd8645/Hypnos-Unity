using UnityEngine;

namespace Hypnos.Resource
{
    public class ResourceConfig : ScriptableObject
    {
        public string appDataPath;
        public string persistentDataPath;
        public string streamingDataPath;

        public string archiveFileExt = ".dat";
        public string archiveBackupFileExt = ".bak";
        public DataArchiverConfig[] dataArchiverConfigs;

        public ResourceLoader resourceLoader;
    }
}