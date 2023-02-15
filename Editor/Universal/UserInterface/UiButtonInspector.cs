using Hypnos.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Hypnos.Editor.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UiButton))]
    public class UiButtonInspector : ButtonEditor
    {
        [MenuItem("GameObject/Dream/UI/Button", false, 1)]
        public static void CreateGameObject()
        {
            // Create a custom game object
            GameObject btnGo = new GameObject("Btn");
            btnGo.AddComponent<RectTransform>();
            GameObject bgGo = new GameObject("Img_Bg");
            bgGo.AddComponent<RectTransform>().SetParent(btnGo.transform);
            btnGo.AddComponent<UiButton>().targetGraphic = bgGo.AddComponent<Image>();

            // Set the selection object as the parent 
            GameObjectUtility.SetParentAndAlign(btnGo, Selection.activeGameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(btnGo, "Create UiButton");
            Selection.activeObject = btnGo;
        }

        protected SerializedProperty objectId;
        protected SerializedProperty onClickSoundId;

        protected override void OnEnable()
        {
            objectId = serializedObject.FindProperty("objectId");
            onClickSoundId = serializedObject.FindProperty("onClickSoundId");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Custom Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(objectId);
            EditorGUILayout.PropertyField(onClickSoundId);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
            EditorGUI.indentLevel--;
            base.OnInspectorGUI();
        }
    }
}