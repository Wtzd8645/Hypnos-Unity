using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    public class KernelEditorConfig : ScriptableObject
    {
        public string[] usedAssamblyNames;

        [Header("Legacy")]
        public string bytesOutputPath;
        public string cSharpCodeNamespace;
        public string dataManagerTypeName;
        public string cSharpCodeOutputPath;

        private void OnEnable()
        {
            usedAssamblyNames ??= new string[4]
            {
                Kernel.UnityEngineAssemblyName,
                KernelEditor.UnityEditorAssemblyName,
                Kernel.FrameworkAssemblyName,
                KernelEditor.FrameworkEditorAssemblyName
            };
        }
    }
}