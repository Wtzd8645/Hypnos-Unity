using System;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class Log : IEquatable<Log>
    {
        public LogType logType;
        public string condition;
        public string stackTrace;

        public int count = 1;
        public int sampleId = 0;

        public Log(LogType type, string condition, string stackTrace)
        {
            logType = type;
            this.condition = condition;
            this.stackTrace = stackTrace;
        }

        public bool Equals(Log other)
        {
            return logType == other.logType &&
                condition == other.condition &&
                stackTrace == other.stackTrace;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine.
            {
                const int OffsetBasis = (int)2166136261;
                const int FnvPrime = 16777619;

                int hash = OffsetBasis;
                hash = (hash * FnvPrime) ^ logType.GetHashCode();
                hash = (hash * FnvPrime) ^ condition.GetHashCode();
                hash = (hash * FnvPrime) ^ stackTrace.GetHashCode();
                return hash;
            }
        }

        public Log ShallowClone()
        {
            return MemberwiseClone() as Log;
        }

        public int GetMemoryUsage()
        {
            return sizeof(LogType) + sizeof(char) * condition.Length + sizeof(char) * stackTrace.Length + sizeof(int) + sizeof(int);
        }
    }
}