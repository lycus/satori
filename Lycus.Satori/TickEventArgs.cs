using System;

namespace Lycus.Satori
{
    public class TickEventArgs : EventArgs
    {
        public bool Idle { get; private set; }

        public TickEventArgs(bool idle)
        {
            Idle = idle;
        }
    }
}
