using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor
{
    public class SelectableFieldDropdown : AdvancedDropdown
    {
        public const string NullTypeName = "Null";

        private static bool IsAssignableNonUnityType(Type type) => TypeUtils.IsFinalAssignableType(type) && !type.IsSubclassOf(typeof(UnityEngine.Object));

        private static List<Type> GetAssignableTypes(SerializedProperty prop)
        {
            Type propType = TypeUtils.GetTypeFromFullName(prop.managedReferenceFieldTypename);
            List<Type> types = TypeCache.GetTypesDerivedFrom(propType).Where(IsAssignableNonUnityType).ToList();
            if (IsAssignableNonUnityType(propType))
            {
                types.Insert(0, propType);
            }

            if (types.Count > 1)
            {
                types.Sort((lhs, rhs) => lhs.Name.CompareTo(rhs.Name));
            }

            types.Insert(0, null);
            return types;
        }

        public static Rect GetRect(Rect rect)
        {
            float offset = EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
            rect.x += offset;
            rect.width -= offset;
            rect.height = EditorGUIUtility.singleLineHeight;
            return rect;
        }

        private readonly SerializedProperty prop;
        private readonly List<Type> assignableTypes;
        private readonly Dictionary<AdvancedDropdownItem, int> indexMap;
        private readonly AdvancedDropdownItem root;
        private readonly Action onSelecteType;

        public SelectableFieldDropdown(AdvancedDropdownState state, SerializedProperty prop, Action onSelecteTypeCb = null) : base(state)
        {
            this.prop = prop;
            assignableTypes = GetAssignableTypes(prop);
            indexMap = new Dictionary<AdvancedDropdownItem, int>(assignableTypes.Count);
            root = new AdvancedDropdownItem("Types");
            for (int i = 0; i < assignableTypes.Count; ++i)
            {
                Type type = assignableTypes[i];
                AdvancedDropdownItem item = new AdvancedDropdownItem(type != null ? type.Name : NullTypeName);
                indexMap.Add(item, i);
                root.AddChild(item);
            }

            onSelecteType = onSelecteTypeCb;
        }

        protected override AdvancedDropdownItem BuildRoot() => root;

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (!indexMap.TryGetValue(item, out int idx))
            {
                return;
            }

            Type newType = assignableTypes[idx];
            if (newType == null)
            {
                ApplyProperty(null);
            }
            else
            {
                object newObject = newType.GetConstructor(Type.EmptyTypes) != null
                    ? Activator.CreateInstance(newType)
                    : FormatterServices.GetUninitializedObject(newType);
                ApplyProperty(newObject);
            }
            onSelecteType?.Invoke();
        }

        private void ApplyProperty(object value)
        {
            prop.managedReferenceValue = value;
            prop.serializedObject.ApplyModifiedProperties();
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
    }
}