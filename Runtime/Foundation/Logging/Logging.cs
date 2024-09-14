using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Blanketmen.Hypnos
{
    public static class Logging
    {
        public const int AllLogChannel = -1;
        public const string DebugLogCondition = "DEBUG_LOG";
        public const string TraceLogCondition = "TRACE_LOG";

        private static Dictionary<int, string> channelNameMap = new Dictionary<int, string>();
        private static int DebugLogChannel = AllLogChannel;
        private static int TraceLogChannel = AllLogChannel;

        public static void SetChannelNames(Type enumType)
        {
            string[] names = Enum.GetNames(enumType);
            Array values = Enum.GetValues(enumType);
            channelNameMap = new Dictionary<int, string>(names.Length);
            for (int i = 0; i < names.Length; ++i)
            {
                channelNameMap.Add((int)values.GetValue(i), $"[{names[i]}] ");
            }
        }

        public static void SetChannel(int debugChn, int traceChn)
        {
            DebugLogChannel = debugChn;
            TraceLogChannel = traceChn;
        }

        public static void Info(string msg, int chn = AllLogChannel)
        {
            if ((chn & DebugLogChannel) != 0)
            {
                channelNameMap.TryGetValue(chn, out string chnName);
                UnityEngine.Debug.Log($"{chnName}{msg}");
            }
        }

        [Conditional(DebugLogCondition)]
        public static void Verbose(string msg, int chn = AllLogChannel)
        {
            if ((chn & DebugLogChannel) != 0)
            {
                channelNameMap.TryGetValue(chn, out string chnName);
                UnityEngine.Debug.Log($"{chnName}{msg}");
            }
        }

        [Conditional(TraceLogCondition)]
        public static void Trace(string msg, int chn = AllLogChannel)
        {
            if ((chn & TraceLogChannel) != 0)
            {
                channelNameMap.TryGetValue(chn, out string chnName);
                UnityEngine.Debug.Log($"{chnName}{msg}");
            }
        }

        [Conditional(DebugLogCondition)]
        public static void Warning(string msg, int chn = AllLogChannel)
        {
            if ((chn & DebugLogChannel) != 0)
            {
                channelNameMap.TryGetValue(chn, out string chnName);
                UnityEngine.Debug.LogWarning($"{chnName}{msg}");
            }
        }

        public static void Error(string msg, string callerName, [CallerMemberName] string memberName = "")
        {
            UnityEngine.Debug.LogError($"[{callerName}.{memberName}] {msg}");
        }
    }
}