using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    public partial class KernelEditor : EditorWindow
    {
        private const string EditorName = "Kernel Editor";
        private const string EditorConfigPath = FrameworkPath + nameof(KernelEditorConfig) + ScriptableObjectExt;

        public static KernelEditorConfig Config { get; private set; }

        public static KernelEditorConfig LoadConfig()
        {
            if (Config == null)
            {
                Config = EditorGUIUtility.Load(EditorConfigPath) as KernelEditorConfig;
            }

            if (Config == null)
            {
                Config = CreateInstance<KernelEditorConfig>();
                Directory.CreateDirectory(EditorBuiltInResourcePath + FrameworkPath);
                AssetDatabase.CreateAsset(Config, EditorBuiltInResourcePath + EditorConfigPath);
                AssetDatabase.Refresh();
            }
            return Config;
        }

        [MenuItem(FrameworkPath + EditorName, false, (int)EditorId.Kernel)]
        private static void ShowWindow()
        {
            GetWindow<KernelEditor>();
        }

        private void Awake()
        {
            titleContent.text = EditorName;
            minSize = new Vector2(480f, 180f);
            LoadConfig();
        }

        private void OnGUI()
        {
            Config = EditorGUILayout.ObjectField("Kernel Editor Config", Config, typeof(KernelEditorConfig), false) as KernelEditorConfig;
        }
    }
}