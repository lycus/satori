using System;

namespace Lycus.Satori
{
    public class InvalidMemoryAccessEventArgs : EventArgs
    {
        [CLSCompliant(false)]
        public uint? Address { get; private set; }

        public bool Write { get; private set; }

        [CLSCompliant(false)]
        public InvalidMemoryAccessEventArgs(uint? address, bool write)
        {
            Address = address;
            Write = write;
        }
    }
}
