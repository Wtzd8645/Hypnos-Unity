using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    public class ScriptableObjectFactoryEditor : EditorWindow
    {
        private const string EditorName = "ScriptableObject Factory";

        private class ScriptableObjectEndNameEditAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
            }
        }

        [MenuItem(KernelEditor.FrameworkPath + EditorName, false, (int)EditorId.ScriptableObjectFactory)]
        public static void ShowWindow()
        {
            GetWindow<ScriptableObjectFactoryEditor>();
        }

        public List<string> assemblyNames;

        private string[] scriptableObjectTypeNames;
        private int selectIndex = -1;

        private SerializedObject serializedObject;
        private SerializedProperty assamblyNamesProp;

        private void Awake()
        {
            titleContent.text = EditorName;
            minSize = new Vector2(480f, 180f);
            KernelEditor.LoadConfig();
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            assamblyNamesProp = serializedObject.FindProperty("assemblyNames");
            SetAssemblies(KernelEditor.Config.usedAssamblyNames);
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(assamblyNamesProp);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateScriptableObjectNames();
            }

            if (GUILayout.Button("Save Assemblies"))
            {
                KernelEditor.Config.usedAssamblyNames = assemblyNames.ToArray();
            }

            if (selectIndex == -1)
            {
                EditorGUILayout.LabelField("Could not find any ScriptableObject.");
                return;
            }

            selectIndex = EditorGUILayout.Popup("ScriptableObject", selectIndex, scriptableObjectTypeNames);
            if (GUILayout.Button("Create"))
            {
                ScriptableObject so = CreateInstance(scriptableObjectTypeNames[selectIndex]);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                    so.GetInstanceID(),
                    CreateInstance<ScriptableObjectEndNameEditAction>(),
                    scriptableObjectTypeNames[selectIndex] + KernelEditor.ScriptableObjectExt,
                    AssetPreview.GetMiniThumbnail(so),
                    null);
            }
        }

        public void SetAssemblies(IList<string> assemblyNames)
        {
            if (assemblyNames == null)
            {
                this.assemblyNames = new List<string>(0);
                return;
            }

            this.assemblyNames = new List<string>(assemblyNames);
            serializedObject.Update();
            UpdateScriptableObjectNames();
        }

        private void UpdateScriptableObjectNames()
        {
            List<Type> soTypes = new List<Type>(64);
            HashSet<string> loadedAssemblySet = new HashSet<string>();
            foreach (string asmName in assemblyNames)
            {
                if (!loadedAssemblySet.Add(asmName))
                {
                    continue;
                }

                try
                {
                    ReflectionUtil.GetTypesFromAssembly(soTypes, typeof(ScriptableObject), Assembly.Load(asmName));
                }
                catch
                {
                    continue;
                }
            }

            selectIndex = soTypes.Count > 0 ? 0 : -1;
            scriptableObjectTypeNames = new string[soTypes.Count];
            for (int i = 0; i < scriptableObjectTypeNames.Length; ++i)
            {
                scriptableObjectTypeNames[i] = soTypes[i].FullName;
            }
        }
    }
}