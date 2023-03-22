using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor.GameData
{
    public class GameDataConverterWindow : EditorWindow
    {
        public const string SettingPathName = "GameDataConverterSettingPath";
        public const string DefaultSettingPath = "Assets/Editor/GameDataConverter/GameDataConverterSetting.asset";

        [SerializeField] private GameDataConverterSetting setting;
        [SerializeField] private List<MetaGameData> metadataList = new List<MetaGameData>(); // NOTE: SerializedProperty must be public.

        private SerializedObject serializedObj;
        private SerializedProperty settingProp;
        private SerializedProperty metadataListProp;

        private GameDataConverter converter = new GameDataConverter();

        private Vector2 scrollViewPos;
        private bool isGenAllCode;
        private bool isGenAllFiles;

        private void Awake()
        {
            titleContent.text = "GameData Converter";
        }

        private void OnEnable()
        {
            serializedObj = new SerializedObject(this);

            string path = EditorPrefs.GetString(SettingPathName, DefaultSettingPath);
            setting = AssetDatabase.LoadAssetAtPath<GameDataConverterSetting>(path);

            settingProp = serializedObj.FindProperty("setting");
            metadataListProp = serializedObj.FindProperty("metadataList");

            GetInfoFromFiles();
        }

        private void OnDisable()
        {
            string path = AssetDatabase.GetAssetPath(setting);
            if (path != EditorPrefs.GetString(SettingPathName, DefaultSettingPath))
            {
                EditorPrefs.SetString(SettingPathName, path);
            }

            serializedObj.Dispose();
            settingProp.Dispose();
            metadataListProp.Dispose();
        }

        private void OnGUI()
        {
            if (setting == null)
            {
                EditorGUILayout.LabelField("Assign Setting File", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(settingProp);
                if (GUILayout.Button("Create Setting"))
                {
                    setting = CreateInstance<GameDataConverterSetting>();
                    AssetDatabase.CreateAsset(setting, DefaultSettingPath);
                    AssetDatabase.Refresh();
                    GetInfoFromFiles();
                }
                serializedObj.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.LabelField("Assign Output Path", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Files Output Path", setting.bytesOutputPath);
            if (GUILayout.Button("Select Folder"))
            {
                string path = EditorUtility.OpenFolderPanel("Files Output Folder", string.Empty, string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    setting.bytesOutputPath = path;
                    EditorUtility.SetDirty(setting);
                }
            }

            EditorGUILayout.LabelField("Code Output Path", setting.cSharpCodeOutputPath);
            if (GUILayout.Button("Select Folder"))
            {
                string path = EditorUtility.OpenFolderPanel("Files Output Folder", string.Empty, string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    setting.cSharpCodeOutputPath = EditorUtility.OpenFolderPanel("Code Output Folder", string.Empty, string.Empty);
                    EditorUtility.SetDirty(setting);
                }
            }

            EditorGUILayout.LabelField("Assign Source Files", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Source Files Path", setting.sourceFilePath);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Folder"))
            {
                string path = EditorUtility.OpenFolderPanel("Files Output Folder", string.Empty, string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    setting.sourceFilePath = EditorUtility.OpenFolderPanel("Source Folder", string.Empty, string.Empty);
                    GetInfoFromFiles();
                    EditorUtility.SetDirty(setting);
                }
            }

            if (GUILayout.Button("Refresh"))
            {
                int index = setting.bytesOutputPath.IndexOf("/Assets");
                string str = setting.bytesOutputPath.Substring(index, setting.bytesOutputPath.Length - index);
                Kernel.Log(str);
                GetInfoFromFiles();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Convert Information", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            float size = SheetInfoDrawer.nameSize - 5f;
            EditorGUILayout.LabelField("File", EditorStyles.boldLabel, GUILayout.Width(size));
            EditorGUILayout.LabelField("Sheet", EditorStyles.boldLabel, GUILayout.Width(size));
            EditorGUILayout.LabelField("Class", EditorStyles.boldLabel, GUILayout.Width(size));
            size = SheetInfoDrawer.toggleWidth - 5f;
            EditorGUILayout.LabelField("GenCode", EditorStyles.boldLabel, GUILayout.Width(size));
            EditorGUILayout.LabelField("GenFile", EditorStyles.boldLabel, GUILayout.Width(size));
            EditorGUILayout.EndHorizontal();

            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);
            for (int i = 0; i < metadataList.Count; i++)
            {
                EditorGUILayout.PropertyField(metadataListProp.GetArrayElementAtIndex(i), true);
            }
            EditorGUILayout.EndScrollView();
            serializedObj.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Is Generate All Code"))
            {
                isGenAllCode = !isGenAllCode;
                for (int i = 0; i < metadataList.Count; i++)
                {
                    metadataList[i].isGenCode = isGenAllCode;
                }
                serializedObj.UpdateIfRequiredOrScript();
            }

            if (GUILayout.Button("Is Generate All File"))
            {
                isGenAllFiles = !isGenAllFiles;
                for (int i = 0; i < metadataList.Count; i++)
                {
                    metadataList[i].isGenFile = isGenAllFiles;
                }
                serializedObj.UpdateIfRequiredOrScript();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Convert"))
            {
                Convert();
            }
        }

        private void GetInfoFromFiles()
        {
            if (setting == null)
            {
                Kernel.LogError("[GameDataConverterWindow] GameDataConverterSetting is null.");
                return;
            }

            string[] externalFiles;
            try
            {
                // NOTE: If the specified extension is exactly three characters long, the method returns files with extensions that begin with the specified extension.
                externalFiles = Directory.GetFiles(setting.sourceFilePath);
            }
            catch (Exception e)
            {
                Kernel.LogError($"[GameDataConverterWindow] {e.Message}");
                return;
            }

            converter.Clear();
            converter.PreParse(externalFiles);
            converter.GetMetadataList(metadataList);
            serializedObj.UpdateIfRequiredOrScript();
        }

        private void Convert()
        {
            if (setting == null)
            {
                Kernel.LogError("[GameDataConverterWindow] GameDataConverterSetting is null.");
                return;
            }

            converter.Parse();
            converter.Convert(setting);
        }
    }
}