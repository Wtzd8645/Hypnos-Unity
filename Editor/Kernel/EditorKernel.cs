using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Hypnos.Editor
{
    [InitializeOnLoad]
    public partial class EditorKernel : EditorWindow
    {
        private const string EditorName = "Editor Kernel";
        private const string DefaultEditorKernelConfigPath = FrameworkPath + "EditorKernelConfig.asset";

        private static EditorKernelConfig config;

        public static EditorKernelConfig Config
        {
            get
            {
                if (config == null)
                {
                    config = EditorGUIUtility.Load(DefaultEditorKernelConfigPath) as EditorKernelConfig;
                }
                return config;
            }
        }

        
        [MenuItem(FrameworkPath + EditorName, false, (int)EditorId.Kernel)]
        private static void ShowWindow()
        {
            if (config == null)
            {
                config = EditorGUIUtility.Load(DefaultEditorKernelConfigPath) as EditorKernelConfig;
            }
            GetWindow<EditorKernel>();
        }

        public static void GetTypesFromAssembly(List<Type> result, Type baseType, Assembly asm)
        {
            if (asm == null)
            {
                return;
            }

            Type[] types = asm.GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                if (types[i].BaseType == baseType)
                {
                    result.Add(types[i]);
                }
            }
        }

        private void Awake()
        {
            titleContent.text = EditorName;
        }

        private void OnGUI()
        {
            config = EditorGUILayout.ObjectField("Editor Kernel Config", config, typeof(EditorKernelConfig), false) as EditorKernelConfig;
        }
    }
}