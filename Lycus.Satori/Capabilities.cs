using System;

namespace Lycus.Satori
{
    [Flags]
    public enum Capabilities
    {
        None = 0x00000000,
        SystemCalls = 0x00000001,
        Debugging = 0x00000002,

        All = SystemCalls | Debugging,
    }
}
