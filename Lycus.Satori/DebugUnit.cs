namespace Lycus.Satori
{
    public sealed class DebugUnit
    {
        public Core Core { get; private set; }

        internal DebugUnit(Core core)
        {
            Core = core;
        }

        internal void Update()
        {
            // TODO: Implement debugging.
        }
    }
}
