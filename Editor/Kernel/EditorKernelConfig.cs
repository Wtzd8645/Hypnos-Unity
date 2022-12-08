using System;
using UnityEngine;

namespace Morpheus.Editor
{
    [Serializable]
    [CreateAssetMenu(fileName = "EditorKernelConfig", menuName = "Editor/EditorKernelConfig")]
    public class EditorKernelConfig : ScriptableObject
    {
        [Header("Legacy")]
        public string bytesOutputPath;
        public string cSharpCodeNamespace;
        public string dataManagerTypeName;
        public string cSharpCodeOutputPath;
    }
}