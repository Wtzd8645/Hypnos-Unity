using Morpheus.GameData;
using Morpheus.UI;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace Morpheus.Editor.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UiText))]
    public class UiTextInspector : TMP_EditorPanelUI
    {
        [MenuItem("GameObject/Dream/UI/Text", false, 0)]
        public static void CreateGameObject()
        {
            // Create a custom game object
            GameObject go = new GameObject("Text");

            // Set the selection object as the parent 
            GameObjectUtility.SetParentAndAlign(go, Selection.activeGameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create UiText");
            Selection.activeObject = go;
            go.AddComponent<RectTransform>();
            go.AddComponent<UiText>();
        }

        protected SerializedProperty styleId;
        protected SerializedProperty i18nTextId;

        protected virtual void Awake()
        {
            if (I18NTextManager.Instance != null)
            {
                return;
            }

            // I18nTextManager.CreateInstance("", "");
        }

        protected override void OnEnable()
        {
            styleId = serializedObject.FindProperty("styleId");
            i18nTextId = serializedObject.FindProperty("i18nTextId");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Custom Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(styleId);
            EditorGUILayout.PropertyField(i18nTextId);
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