using Morpheus.UI;
using UnityEditor;
using UnityEditor.UI;

namespace Morpheus.Editor.UnityExtension.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SnapScrollRect))]
    public class SnapScrollRectInspector : ScrollRectEditor
    {
        private SerializedProperty swipeDirection;
        private SerializedProperty isAutoCalculateSize;
        private SerializedProperty pageSize;
        private SerializedProperty pageSpace;
        private SerializedProperty totalPageIndex;
        private SerializedProperty swipeThreshold;
        private SerializedProperty swipeSpeed;

        protected virtual void Awake()
        {
            swipeDirection = serializedObject.FindProperty("swipeDirection");
            isAutoCalculateSize = serializedObject.FindProperty("isAutoCalculateSize");
            pageSize = serializedObject.FindProperty("pageSize");
            pageSpace = serializedObject.FindProperty("pageSpace");
            totalPageIndex = serializedObject.FindProperty("totalPageIndex");
            swipeThreshold = serializedObject.FindProperty("swipeThreshold");
            swipeSpeed = serializedObject.FindProperty("swipeSpeed");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("Snap Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(swipeDirection);
            EditorGUILayout.PropertyField(isAutoCalculateSize);
            EditorGUILayout.PropertyField(pageSize);
            EditorGUILayout.PropertyField(pageSpace);
            EditorGUILayout.PropertyField(totalPageIndex);
            EditorGUILayout.PropertyField(swipeThreshold);
            EditorGUILayout.PropertyField(swipeSpeed);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
            EditorGUI.indentLevel--;
        }
    }
}