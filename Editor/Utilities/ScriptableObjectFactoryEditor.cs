using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Morpheus.Editor
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
        private static readonly Type ScriptableObjectType = typeof(ScriptableObject);

        private static ScriptableObjectEndNameEditAction endNameEditActionHandler;

        [MenuItem("Assets/Create/ScriptableObject")]
        public static void ShowWindow()
        {
            GetWindow<ScriptableObjectFactoryEditor>().ShowPopup();
        }

        private string[] scriptableObjectTypeNames;
        private int selectIndex;

        private void Awake()
        {
            titleContent.text = "ScriptableObject Factory";
        }

        private void OnEnable()
        {
            if (endNameEditActionHandler == null)
            {
                endNameEditActionHandler = CreateInstance<ScriptableObjectEndNameEditAction>();
            }

            GetScriptableObjectNames();
        }

        private void OnGUI()
        {
            selectIndex = EditorGUILayout.Popup("Scriptable Object", selectIndex, scriptableObjectTypeNames);
            if (GUILayout.Button("Create"))
            {
                ScriptableObject so = CreateInstance(scriptableObjectTypeNames[selectIndex]);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                    so.GetInstanceID(),
                    endNameEditActionHandler,
                    scriptableObjectTypeNames[selectIndex] + ScriptableObjectExt,
                    AssetPreview.GetMiniThumbnail(so),
                    null);

                Close();
            }
        }

        private void GetScriptableObjectNames()
        {
            List<Type> types = new List<Type>(64);
            EditorKernel.GetTypesFromAssembly(types, ScriptableObjectType, EditorKernel.CSharpAssembly);
            EditorKernel.GetTypesFromAssembly(types, ScriptableObjectType, EditorKernel.CSharpEditorAssembly);
            EditorKernel.GetTypesFromAssembly(types, ScriptableObjectType, EditorKernel.DreamFrameworkAssembly);

            scriptableObjectTypeNames = new string[types.Count];
            for (int i = 0; i < scriptableObjectTypeNames.Length; ++i)
            {
                scriptableObjectTypeNames[i] = types[i].FullName;
            }
        }
    }
}