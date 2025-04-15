using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    [CustomPropertyDrawer(typeof(SelectableField), true)]
    public class SelectableFieldPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty ddProp;
        private SelectableFieldDropdown dropdown;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                DrawProperty(rect, property, label);
                return;
            }

            DrawTypeDropdown(rect, property, label);
            DrawProperty(rect, property, label);
        }

        protected virtual void DrawTypeDropdown(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect ddRect = SelectableFieldDropdown.GetRect(rect);
            object refVal = prop.managedReferenceValue;
            GUIContent ddCont = new GUIContent(refVal != null ? refVal.GetType().Name : SelectableFieldDropdown.NullTypeName);
            if (!EditorGUI.DropdownButton(ddRect, ddCont, FocusType.Keyboard))
            {
                return;
            }

            if (prop != ddProp)
            {
                ddProp = prop;
                dropdown = new SelectableFieldDropdown(new AdvancedDropdownState(), prop);
            }

            dropdown.Show(ddRect);
        }

        protected virtual void DrawProperty(Rect rect, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.PropertyField(rect, prop, label, true);
        }
    }
}