using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class ResourceConfig : ScriptableObject
    {
        public string appDataPath;
        public string persistentDataPath;
        public string streamingDataPath;

        public ResourceLoader resourceLoader;
        public ArchiverConfig assetRegistryArchiverConfig;
    }
}