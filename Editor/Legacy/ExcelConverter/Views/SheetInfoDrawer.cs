using UnityEditor;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    [CustomPropertyDrawer(typeof(SheetInfo))]
    public class SheetInfoDrawer : PropertyDrawer
    {
        public const float rowHeight = 16f;

        public const float nameWidth = 184f;
        public const float nameSize = nameWidth + 16f;

        public const float toggleWidth = 80f;
        public const float toggleCenterOffset = toggleWidth / 4f;
        //public const float toggleCenterWidth = toggleWidth * 1.5f;
        //private const float nameSize = nameWidth + 16f;

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

            //EditorGUIUtility.labelWidth = 60;
            rect = new Rect(rect.x + nameSize, position.y, nameWidth, rowHeight);
            prop = property.FindPropertyRelative("className");
            EditorGUI.LabelField(rect, new GUIContent(prop.stringValue));
            //
            EditorGUIUtility.labelWidth = 80f;
            SerializedProperty genCodeProp = property.FindPropertyRelative("isGenCode");
            rect = new Rect(rect.x + nameSize + toggleCenterOffset, position.y, toggleWidth, rowHeight);
            genCodeProp.boolValue = EditorGUI.Toggle(rect, genCodeProp.boolValue);
            //genCodeProp.boolValue = EditorGUI.Toggle(rect, "GenCode", genCodeProp.boolValue);

            //EditorGUIUtility.labelWidth = 80;
            rect = new Rect(rect.x + toggleWidth, position.y, toggleWidth, rowHeight);
            SerializedProperty genFileProp = property.FindPropertyRelative("isGenFile");
            genFileProp.boolValue = EditorGUI.Toggle(rect, genFileProp.boolValue);
            //genFileProp.boolValue = EditorGUI.Toggle(rect, "GenFile", genFileProp.boolValue);

            EditorGUI.EndProperty();
        }
    }
}