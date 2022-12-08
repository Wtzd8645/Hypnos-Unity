using UnityEditor;
using UnityEngine;

namespace Morpheus.Editor.GameData
{
    [CustomPropertyDrawer(typeof(MetaGameData))]
    public class MetaGameDataDrawer : PropertyDrawer
    {
        public const float rowHeight = 16f;

        public const float nameWidth = 184f;
        public const float nameSize = nameWidth + 16f;

        public const float toggleWidth = 80f;
        public const float toggleCenterOffset = toggleWidth / 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect rect = new Rect(position.x, position.y, nameWidth, rowHeight);
            SerializedProperty prop = property.FindPropertyRelative("fileName");
            EditorGUI.LabelField(rect, new GUIContent(prop.stringValue));

            //EditorGUIUtility.labelWidth = 60;
            rect = new Rect(rect.x + nameSize, position.y, nameWidth, rowHeight);
            prop = property.FindPropertyRelative("sheetName");
            EditorGUI.LabelField(rect, new GUIContent(prop.stringValue));

            rect = new Rect(rect.x + nameSize, position.y, nameWidth, rowHeight);
            prop = property.FindPropertyRelative("dataClass");
            EditorGUI.LabelField(rect, new GUIContent(prop.stringValue));

            SerializedProperty genCodeProp = property.FindPropertyRelative("isGenCode");
            rect = new Rect(rect.x + nameSize + toggleCenterOffset, position.y, toggleWidth, rowHeight);
            genCodeProp.boolValue = EditorGUI.Toggle(rect, genCodeProp.boolValue);

            rect = new Rect(rect.x + toggleWidth, position.y, toggleWidth, rowHeight);
            SerializedProperty genFileProp = property.FindPropertyRelative("isGenFile");
            genFileProp.boolValue = EditorGUI.Toggle(rect, genFileProp.boolValue);

            EditorGUI.EndProperty();
        }
    }
}