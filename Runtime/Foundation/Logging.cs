using System;
using System.Diagnostics;

namespace Blanketmen.Hypnos
{
    [Flags]
    public enum LogChannel
    {
        None = 0,
        Environment = 1,
        Resource = 1 << 1,
        Network = 1 << 2,
        Input = 1 << 3,
        GameTime = 1 << 4,
        GameState = 1 << 5,
        UI = 1 << 6,
        Audio = 1 << 7,
        All = -1
    }

    public static class Logging
    {
        public const int AllLogChannel = -1;
        public const string TraceLogCondition = "TRACE_LOG";
        public const string DebugLogCondition = "DEBUG_LOG";

        public static int TraceLogChannel = AllLogChannel;
        public static int DebugLogChannel = AllLogChannel;

        public static void LogFile(string message, int channel = AllLogChannel)
        {
            // TODO: Implement
        }

        [Conditional(TraceLogCondition)]
        public static void LogTrace(string message, int channel = AllLogChannel)
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