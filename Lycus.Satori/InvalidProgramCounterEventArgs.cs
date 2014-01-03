using System;

namespace Lycus.Satori
{
    public class InvalidProgramCounterEventArgs : EventArgs
    {
        [CLSCompliant(false)]
        public uint Value { get; private set; }

        [CLSCompliant(false)]
        public InvalidProgramCounterEventArgs(uint value)
        {
            Value = value;
        }
    }
}
