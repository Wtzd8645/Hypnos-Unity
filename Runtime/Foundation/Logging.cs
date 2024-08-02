using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Blanketmen.Hypnos
{
    public static class Logging
    {
        public const int AllLogChannel = -1;
        public const string DebugLogCondition = "DEBUG_LOG";
        public const string TraceLogCondition = "TRACE_LOG";

        public static int DebugLogChannel = AllLogChannel;
        public static int TraceLogChannel = AllLogChannel;

        public static void Info(string msg, int chn = AllLogChannel)
        {
            if ((chn & DebugLogChannel) != 0)
            {
                UnityEngine.Debug.Log(msg);
            }
        }

        [Conditional(DebugLogCondition)]
        public static void Verbose(string msg, int chn = AllLogChannel)
        {
            if ((chn & DebugLogChannel) != 0)
            {
                UnityEngine.Debug.Log(msg);
            }
        }

        [Conditional(TraceLogCondition)]
        public static void Trace(string msg, int chn = AllLogChannel)
        {
            if ((chn & TraceLogChannel) != 0)
            {
                UnityEngine.Debug.Log(msg);
            }
        }

        [Conditional(DebugLogCondition)]
        public static void Warning(string msg, int chn = AllLogChannel)
        {
            if ((chn & DebugLogChannel) != 0)
            {
                UnityEngine.Debug.LogWarning(msg);
            }
        }

        public static void Error(string msg, [CallerMemberName] string name = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNum = 0)
        {
            UnityEngine.Debug.LogError($"{msg}\n{name}\n{filePath}:{lineNum}");
        }

        public static void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }
    }
}