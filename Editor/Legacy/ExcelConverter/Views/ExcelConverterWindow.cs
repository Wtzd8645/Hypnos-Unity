using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    public class ExcelConverterWindow : EditorWindow
    {
        public const string SettingPathName = "ExcelConverterSettingPath";
        public const string DefaultSettingPath = "Assets/Editor/ExcelConverter/ExcelConverterSetting.asset";

        #region Static Member
        private static bool IsGenXML;
        private static bool IsEncrypt;
        private static bool IsGenCSharpCode;
        #endregion

        public ExcelConverterSetting setting;

        // SerializedProperty must be public.
        public List<SheetInfo> sheetInfos;

        private ExcelConverter converter;
        private SerializedObject serializedObj;
        private SerializedProperty settingProp;
        private SerializedProperty sheetInfoProp;

        private Vector2 scrollViewPos;
        private bool isGenAllCode;
        private bool isGenAllFiles;

        private void Awake()
        {
            titleContent.text = "Excel Converter";
        }

        private void OnEnable()
        {
            string path = EditorPrefs.GetString(SettingPathName, DefaultSettingPath);
            setting = AssetDatabase.LoadAssetAtPath<ExcelConverterSetting>(path);

            sheetInfos = new List<SheetInfo>(32);
            converter = new ExcelConverter();
            serializedObj = new SerializedObject(this);
            settingProp = serializedObj.FindProperty("setting");
            sheetInfoProp = serializedObj.FindProperty("sheetInfos");

            GetSheetInfoFromExcel();
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
            sheetInfoProp.Dispose();
        }

        private void OnGUI()
        {
            if (setting == null)
            {
                EditorGUILayout.LabelField("Assign Setting File", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(settingProp);
                if (GUILayout.Button("Create Setting"))
                {
                    setting = CreateInstance<ExcelConverterSetting>();
                    AssetDatabase.CreateAsset(setting, DefaultSettingPath);
                    AssetDatabase.Refresh();
                    GetSheetInfoFromExcel();
                }
                serializedObj.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.LabelField("Assign Output Path", EditorStyles.boldLabel);
            //EditorGUILayout.BeginVertical();
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Binary Output Path", setting.bytesOutputPath);
            if (GUILayout.Button("Select Folder"))
            {
                setting.bytesOutputPath = EditorUtility.OpenFolderPanel("Excel Output Folder", "", "");
                EditorUtility.SetDirty(setting);
            }
            //EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("C# Code Output Path", setting.cSharpCodeOutputPath);
            if (GUILayout.Button("Select Folder"))
            {
                setting.cSharpCodeOutputPath = EditorUtility.OpenFolderPanel("C# Code Output Folder", "", "");
                EditorUtility.SetDirty(setting);
            }
            //EditorGUILayout.EndHorizontal();
            //EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("Assign Excel Files", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Excel Source Path", setting.excelSourcePath);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Folder"))
            {
                setting.excelSourcePath = EditorUtility.OpenFolderPanel("Source Folder", "", "");
                GetSheetInfoFromExcel();
                EditorUtility.SetDirty(setting);
            }
            if (GUILayout.Button("Refresh"))
            {
                GetSheetInfoFromExcel();
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
            for (int i = 0; i < sheetInfos.Count; i++)
            {
                EditorGUILayout.PropertyField(sheetInfoProp.GetArrayElementAtIndex(i), true);
            }
            EditorGUILayout.EndScrollView();
            serializedObj.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Is Generate All Code"))
            {
                isGenAllCode = !isGenAllCode;
                for (int i = 0; i < sheetInfos.Count; i++)
                {
                    sheetInfos[i].isGenCode = isGenAllCode;
                }
                serializedObj.UpdateIfRequiredOrScript();
            }

            if (GUILayout.Button("Is Generate All File"))
            {
                isGenAllFiles = !isGenAllFiles;
                for (int i = 0; i < sheetInfos.Count; i++)
                {
                    sheetInfos[i].isGenFile = isGenAllFiles;
                }
                serializedObj.UpdateIfRequiredOrScript();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Convert"))
            {
                Convert();
            }
        }

        private void GetSheetInfoFromExcel()
        {
            if (setting == null)
            {
                Logging.Error("setting is null.", nameof(ExcelConverterWindow));
                return;
            }

            string[] excelFiles;
            try
            {
                // NOTE: If the specified extension is exactly three characters long, the method returns files with extensions that begin with the specified extension.
                excelFiles = Directory.GetFiles(setting.excelSourcePath, "*" + ExcelConverter.XlsExtension);
            }
            catch (Exception e)
            {
                Logging.Error($"{e.Message}", nameof(ExcelConverterWindow));
                return;
            }

            sheetInfos.Clear();
            for (int i = 0; i < excelFiles.Length; i++)
            {
                List<SheetInfo> sheetInfo = converter.GetSheetInfo(excelFiles[i]);
                if (sheetInfo != null)
                {
                    sheetInfos.AddRange(sheetInfo);
                }
            }

            serializedObj.UpdateIfRequiredOrScript();
        }

        // TODO: Maybe to fix something.
        private void Convert()
        {
            if (string.IsNullOrEmpty(setting.bytesOutputPath))
            {
                Logging.Error("Output path is empty.", nameof(ExcelConverterWindow));
                return;
            }

            converter.Convert(sheetInfos, setting.cSharpCodeOutputPath, setting.bytesOutputPath);
        }
    }
}