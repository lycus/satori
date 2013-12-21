using System;

namespace Lycus.Satori
{
    public class InvalidInstructionEventArgs : EventArgs
    {
        [CLSCompliant(false)]
        public uint Address { get; private set; }

        [CLSCompliant(false)]
        public uint Value { get; private set; }

        [CLSCompliant(false)]
        public InvalidInstructionEventArgs(uint address, uint value)
        {
            Address = address;
            Value = value;
        }
    }
}
