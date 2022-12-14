using System.Diagnostics;

namespace Morpheus
{
    public static class Logger
    {
        public const int AllLogChannel = -1;
        public const string TraceLogCondition = "TRACE_LOG";
        public const string DebugLogCondition = "DEBUG_LOG";

        public static int TraceLogChannel = AllLogChannel;
        public static int DebugLogChannel = AllLogChannel;

        public static void FileLog(string message, int channel = AllLogChannel)
        {
            // TODO: Implement
        }

        [Conditional(TraceLogCondition)]
        public static void TraceLog(string message, int channel = AllLogChannel)
        {
            if ((channel & TraceLogChannel) != 0)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        [Conditional(DebugLogCondition)]
        public static void Log(string message, int channel = AllLogChannel)
        {
            if ((channel & DebugLogChannel) != 0)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        [Conditional(DebugLogCondition)]
        public static void LogWarning(string message, int channel = AllLogChannel)
        {
            if ((channel & DebugLogChannel) != 0)
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        [Conditional(DebugLogCondition)]
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional(DebugLogCondition)]
        public static void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }
    }
}