using System;

namespace Blanketmen.Hypnos
{
    [Flags]
    public enum RunningVersion
    {
        Release = 0,
        Stage = 1,
        Development = 2,
        Test = 3
    }

    public partial class Kernel
    {
        public const string UnityEngineAssemblyName = "Assembly-CSharp";
        public const string FrameworkAssemblyName = "Hypnos";

#if TEST
        public const RunningVersion Version = RunningVersion.Test;
#elif DEVELOPMENT
        public const RunningVersion Version = RunningVersion.Development;
#elif STAGE
        public const RunningVersion Version = RunningVersion.Stage;
#else
        public const RunningVersion Version = RunningVersion.Release;
#endif
    }
}