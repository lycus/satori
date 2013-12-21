namespace Lycus.Satori
{
    public sealed class DirectAccessEngine
    {
        public Core Core { get; private set; }

        internal DirectAccessEngine(Core core)
        {
            Core = core;
        }

        internal void Update()
        {
        }
    }
}
