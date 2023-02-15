using System;
using UnityEngine;

namespace Hypnos.Editor
{
    [Serializable]
    [CreateAssetMenu(fileName = "EditorKernelConfig", menuName = "Editor/EditorKernelConfig")]
    public class EditorKernelConfig : ScriptableObject
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
                EditorKernel.UnityEditorAssemblyName,
                Kernel.FrameworkAssemblyName,
                EditorKernel.FrameworkEditorAssemblyName
            };
        }
    }
}