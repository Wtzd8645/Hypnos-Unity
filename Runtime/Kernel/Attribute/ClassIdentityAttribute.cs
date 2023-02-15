using System;

namespace Hypnos
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ClassIdentityAttribute : Attribute
    {
        public readonly int Id;
        public readonly RunningVersion Version;

        public ClassIdentityAttribute(int id, RunningVersion version = RunningVersion.Release)
        {
            Id = id;
            Version = version;
        }
    }
}