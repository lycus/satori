namespace Lycus.Satori
{
    public sealed class DebugUnit
    {
        public Core Core { get; private set; }

        internal DebugUnit(Core core)
        {
            Core = core;
        }

        internal bool Update()
        {
            // TODO: We're disabling the debug unit entirely for now.
            return false;
        }
    }
}
