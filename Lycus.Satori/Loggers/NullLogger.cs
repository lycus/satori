namespace Lycus.Satori.Loggers
{
    /// <summary>
    /// A logger that simply discards messages.
    /// </summary>
    public sealed class NullLogger : Logger
    {
        protected override void OnVerboseLog(string message)
        {
        }

        protected override void OnDebugLog(string message)
        {
        }

        protected override void OnTraceLog(string message)
        {
        }
    }
}
