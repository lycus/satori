using System;

namespace Lycus.Satori.Loggers
{
    public sealed class ConsoleLogger : Logger
    {
        protected override void OnVerboseLog(string message)
        {
            Console.Error.WriteLine(message);
        }

        protected override void OnDebugLog(string message)
        {
            Console.Error.WriteLine(message);
        }

        protected override void OnTraceLog(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
