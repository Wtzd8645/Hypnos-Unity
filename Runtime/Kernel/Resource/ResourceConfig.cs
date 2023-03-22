using UnityEngine;

namespace Blanketmen.Hypnos
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