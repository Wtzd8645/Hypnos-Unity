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
        private const string DefaultEditorKernelConfigPath = KernelDirectory + "EditorKernelConfig.asset";

        private static EditorKernelConfig config;

        public static Assembly CSharpAssembly { get; }
        public static Assembly CSharpEditorAssembly { get; }
        public static Assembly DreamFrameworkAssembly { get; }
        public static EditorKernelConfig Config => config;

        static EditorKernel()
        {
            config = EditorGUIUtility.Load(DefaultEditorKernelConfigPath) as EditorKernelConfig;
            if (config == null)
            {
                Kernel.LogError("[EditorKernel] Can't load EditorKernelConfig.");
            }

            //CSharpAssembly = Assembly.Load(AppKernel.CSharpAssembly);
            //CSharpEditorAssembly = Assembly.Load(CSharpEditorAssemblyName);
            //DreamFrameworkAssembly = Assembly.Load(AppKernel.KernelAssembly);
            Kernel.Log("[EditorKernel] Initialized");
        }

        // There will have gap when priority have difference of 11.
        [MenuItem(FrameworkMenuDirectory + EditorName, false, 98)]
        private static void ShowWindow()
        {
            GetWindow<EditorKernel>();
        }

        public static void GetTypesFromAssembly(List<Type> result, Type type, Assembly asm)
        {
            Type[] types = asm.GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                if (types[i].BaseType == type)
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