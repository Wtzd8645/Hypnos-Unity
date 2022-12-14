using System;
using UnityEngine;

namespace Morpheus.Debug
{
    public class Log : IEquatable<Log>
    {
        public LogType LogType;
        public string Condition;
        public string StackTrace;

        public int Count = 1;
        public int SampleId = 0;

        public Log(LogType type, string condition, string stackTrace)
        {
            LogType = type;
            this.Condition = condition;
            this.StackTrace = stackTrace;
        }

        public bool Equals(Log other)
        {
            return LogType == other.LogType &&
                Condition == other.Condition &&
                StackTrace == other.StackTrace;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine.
            {
                const int OffsetBasis = (int)2166136261;
                const int FnvPrime = 16777619;

                int hash = OffsetBasis;
                hash = (hash * FnvPrime) ^ LogType.GetHashCode();
                hash = (hash * FnvPrime) ^ Condition.GetHashCode();
                hash = (hash * FnvPrime) ^ StackTrace.GetHashCode();
                return hash;
            }
        }

        public Log ShallowClone()
        {
            return MemberwiseClone() as Log;
        }

        public int GetMemoryUsage()
        {
            return sizeof(LogType) + sizeof(char) * Condition.Length + sizeof(char) * StackTrace.Length + sizeof(int) + sizeof(int);
        }
    }
}