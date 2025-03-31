using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class ArchiveConfig : ScriptableObject
    {
        public string persistentDataPath;

        public string archiveFileExt = ".dat";
        public string archiveBackupFileExt = ".bak";
    }
}