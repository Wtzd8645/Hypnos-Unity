using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Blanketmen.Hypnos.Editor
{
    public partial class KernelEditor
    {
        /// <summary>
        /// Console Log Redirector
        /// </summary>
        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            const string LogScriptName = "Kernel.Logger";

            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj.name != LogScriptName)
            {
                return false;
            }

            GetFileInfo(out string assetPath, out line);
            if (string.IsNullOrEmpty(assetPath) || line == -1)
            {
                return false;
            }

            obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            AssetDatabase.OpenAsset(obj, line);
            return true;
        }

        private static string GetConsoleLog()
        {
            const string ConsoleWindowClass = "UnityEditor.ConsoleWindow";
            const string ConsoleWindowField = "ms_ConsoleWindow";
            const BindingFlags ConsoleWindowBindingFlag = BindingFlags.Static | BindingFlags.NonPublic;
            const string ActiveTextField = "m_ActiveText";
            const BindingFlags ActiveTextBindingFlag = BindingFlags.Instance | BindingFlags.NonPublic;

            Type consoleWindowType = typeof(EditorWindow).Assembly.GetType(ConsoleWindowClass);
            FieldInfo consoleWindowField = consoleWindowType.GetField(ConsoleWindowField, ConsoleWindowBindingFlag);
            object consoleWindowInst = consoleWindowField.GetValue(null);

            if (consoleWindowInst == null || (EditorWindow)consoleWindowInst != EditorWindow.focusedWindow)
            {
                return string.Empty;
            }

            FieldInfo activeTextfield = consoleWindowType.GetField(ActiveTextField, ActiveTextBindingFlag);
            return activeTextfield.GetValue(consoleWindowInst).ToString();
        }

        private static void GetFileInfo(out string filePath, out int fileLine)
        {
            const string UnityLogTag = "UnityEngine.Debug:";
            const string LogStartTag = "(at ";
            const string LogEndTag = ".cs";

            string consoleLog = GetConsoleLog();
            string[] context = consoleLog.Split('\n');
            for (int i = context.Length - 1; i >= 0; --i)
            {
                if (context[i].Contains(UnityLogTag))
                {
                    consoleLog = context[i + 2]; // NOTE: Get the previous script of the logger script.
                    break;
                }
            }

            int startIndex = consoleLog.LastIndexOf(LogStartTag);
            int endIndex = consoleLog.LastIndexOf(LogEndTag);
            if (startIndex == -1 || endIndex == -1)
            {
                filePath = null;
                fileLine = -1;
                return;
            }

            startIndex += LogStartTag.Length;
            endIndex += LogEndTag.Length;
            filePath = consoleLog.Substring(startIndex, endIndex - startIndex);

            consoleLog = consoleLog.Substring(++endIndex, consoleLog.Length - endIndex - 1);
            fileLine = int.Parse(consoleLog);
        }
    }
}