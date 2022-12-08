using System;

namespace Morpheus
{
    [Flags]
    public enum RunningVersion
    {
        Release = 0,
        Stage = 1,
        Development = 2,
        Test = 3
    }

    [Flags]
    public enum DebugLogChannel
    {
        None = 0,
        GameTime = 1,
        Resource = GameTime << 1,
        Input = Resource << 1,
        Network = Input << 1,
        GameState = Network << 1,
        Audio = GameState << 1,
        UI = Audio << 1,
        All = -1
    }

    public partial class AppKernel
    {
        public const string CSharpAssembly = "Assembly-CSharp";
        public const string KernelAssembly = "Morpheus";

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