using System;

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
        GameState = 1 << 4,
        UI = 1 << 5,
        Audio = 1 << 6,
        All = -1
    }
}