using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    // NOTE: There will have gap when priority have difference of 11.
    public enum EditorId
    {
        Build,
        ScriptableObjectFactory
    }

    public class EditorEnvironmentSettings : ScriptableObject
    {
        public const string UnityEditorAssemblyName = "Assembly-CSharp-Editor";
        public const string FrameworkEditorAssemblyName = "Blanketmen.Hypnos.Editor";

        public const string ScriptableObjectExt = ".asset";

        public const string EditorBuiltInResourcePath = "Assets/Editor Default Resources/";
        public const string FrameworkPath = "Blanketmen/Hypnos/";

        public const string EditorConfigPath = FrameworkPath + nameof(EditorEnvironmentSettings) + ScriptableObjectExt;

        private static EditorEnvironmentSettings instance;

        public static EditorEnvironmentSettings Instance
        { 
            get
            {
                if (instance == null)
                {
                    instance = EditorGUIUtility.Load(EditorConfigPath) as EditorEnvironmentSettings;
                }

                if (instance == null)
                {
                    instance = CreateInstance<EditorEnvironmentSettings>();
                    Directory.CreateDirectory(EditorBuiltInResourcePath + FrameworkPath);
                    AssetDatabase.CreateAsset(Instance, EditorBuiltInResourcePath + EditorConfigPath);
                    AssetDatabase.Refresh();
                }
                return instance;
            }
        }

        public string[] usedAssamblyNames;
        
        private void OnEnable()
        {
            usedAssamblyNames ??= new string[4]
            {
                EnvironmentManager.UnityEngineAssemblyName,
                UnityEditorAssemblyName,
                EnvironmentManager.FrameworkAssemblyName,
                FrameworkEditorAssemblyName
            };
        }
    }
}