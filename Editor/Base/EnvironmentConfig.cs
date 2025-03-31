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

    public class EnvironmentConfig : ScriptableObject
    {
        public const string RootDir = "Assets/Editor Default Resources/";
        public const string ProjectDir = "Blanketmen/Hypnos/";
        public const string EditorResourceDir = RootDir + ProjectDir;

        public const string ConfigurationFileName = nameof(EnvironmentConfig) + ScriptableObjectExt;

        public const string UnityEditorAssemblyName = "Assembly-CSharp-Editor";
        public const string FrameworkEditorAssemblyName = "Blanketmen.Hypnos.Editor";

        public const string ScriptableObjectExt = ".asset";

        private static EnvironmentConfig instance;

        public static EnvironmentConfig Instance
        { 
            get
            {
                if (instance == null)
                {
                    instance = EditorGUIUtility.Load(ProjectDir + ConfigurationFileName) as EnvironmentConfig;
                }

                if (instance == null)
                {
                    instance = CreateInstance<EnvironmentConfig>();
                    Directory.CreateDirectory(EditorResourceDir);
                    AssetDatabase.CreateAsset(instance, EditorResourceDir + ConfigurationFileName);
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