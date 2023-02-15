using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Hypnos.Editor
{
    public static class ConsoleLogRedirector
    {
        private static readonly string logScriptName = "DebugLogger"; // TODO: Get from config.

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj.name != logScriptName)
            {
                return false;
            }

            string consoleLog = GetConsoleLog();
            GetFileInfo(consoleLog, out string assetPath, out line);
            if (string.IsNullOrEmpty(assetPath) || line == -1)
            {
                return false;
            }

            obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
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

            System.Type consoleWindowType = typeof(EditorWindow).Assembly.GetType(ConsoleWindowClass);
            FieldInfo consoleWindowfield = consoleWindowType.GetField(ConsoleWindowField, ConsoleWindowBindingFlag);
            object consoleWindowInst = consoleWindowfield.GetValue(null);

            string consoleLog = string.Empty;
            if (consoleWindowInst == null)
            {
                return consoleLog;
            }

            if ((Object)consoleWindowInst != EditorWindow.focusedWindow)
            {
                return consoleLog;
            }

            FieldInfo activeTextfield = consoleWindowType.GetField(ActiveTextField, ActiveTextBindingFlag);
            consoleLog = activeTextfield.GetValue(consoleWindowInst).ToString();
            return consoleLog;
        }

        private static void GetFileInfo(string consoleLog, out string filePath, out int fileLine)
        {
            const string UnityLogTag = "UnityEngine.Debug:";
            const string LogStartTag = "(at ";
            const string LogEndTag = ".cs";

            string[] context = consoleLog.Split('\n');
            for (int i = context.Length - 1; i >= 0; --i)
            {
                if (context[i].Contains(UnityLogTag))
                {
                    consoleLog = context[i + 2];
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