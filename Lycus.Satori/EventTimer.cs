namespace Lycus.Satori
{
    public sealed class EventTimer
    {
        public Core Core { get; private set; }

        internal EventTimer(Core core)
        {
            Core = core;
        }

        internal void IncrementClockCycles()
        {
            // TODO: Implement timers.
        }

        internal void IncrementIdleClockCycles()
        {
        }

        internal void IncrementInstructions(bool ialu)
        {
        }
    }
}
