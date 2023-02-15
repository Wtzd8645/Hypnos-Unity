using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Hypnos.Editor
{
    public class ScriptableObjectFactoryEditor : EditorWindow
    {
        private class ScriptableObjectEndNameEditAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
            }
        }

        private const string ScriptableObjectExt = ".asset";

        [MenuItem(EditorKernel.FrameworkPath + " Scriptable Object Factory", false, (int)EditorId.ScriptableObjectFactory)]
        public static void ShowWindow()
        {
            ScriptableObjectFactoryEditor window = GetWindow<ScriptableObjectFactoryEditor>();
            window.SetAssemblies(EditorKernel.Config != null ? EditorKernel.Config.usedAssamblyNames : null);
            window.ShowPopup();
        }

        public List<string> assemblyNames;

        private string[] scriptableObjectTypeNames;
        private int selectIndex = -1;

        private SerializedObject serializedObject;
        private SerializedProperty assamblyNamesProp;

        private void Awake()
        {
            titleContent.text = "ScriptableObject Factory";
            minSize = new Vector2(480f, 180f);
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            assamblyNamesProp = serializedObject.FindProperty("assemblyNames");
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
                if (EditorKernel.Config == null)
                {
                    Kernel.LogError("[ScriptableObjectFactoryEditor] EditorKernelConfig is not loaded.");
                }
                else
                {
                    EditorKernel.Config.usedAssamblyNames = assemblyNames.ToArray();
                }
            }

            if (selectIndex == -1)
            {
                EditorGUILayout.LabelField("Could not find any ScriptableObject.");
                return;
            }

            selectIndex = EditorGUILayout.Popup("Scriptable Object", selectIndex, scriptableObjectTypeNames);
            if (GUILayout.Button("Create"))
            {
                ScriptableObject so = CreateInstance(scriptableObjectTypeNames[selectIndex]);
                ScriptableObjectEndNameEditAction endNameEditAction = CreateInstance<ScriptableObjectEndNameEditAction>();
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                    so.GetInstanceID(),
                    endNameEditAction,
                    scriptableObjectTypeNames[selectIndex] + ScriptableObjectExt,
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
            List<Type> types = new List<Type>(64);
            foreach (string asmName in assemblyNames)
            {
                try
                {
                    EditorKernel.GetTypesFromAssembly(types, typeof(ScriptableObject), Assembly.Load(asmName));
                }
                catch
                {
                    continue;
                }
            }

            selectIndex = types.Count > 0 ? 0 : -1;
            scriptableObjectTypeNames = new string[types.Count];
            for (int i = 0; i < scriptableObjectTypeNames.Length; ++i)
            {
                scriptableObjectTypeNames[i] = types[i].FullName;
            }
        }
    }
}