using System;
using System.IO;

namespace Lycus.Satori.Loggers
{
    public sealed class StreamLogger : Logger
    {
        public StreamWriter Writer { get; private set; }

        bool _disposed;

        public StreamLogger(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            Writer = new StreamWriter(stream);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                Writer.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnVerboseLog(string message)
        {
            Writer.WriteLine(message);
        }

        protected override void OnDebugLog(string message)
        {
            Writer.WriteLine(message);
        }

        protected override void OnTraceLog(string message)
        {
            Writer.WriteLine(message);
        }
    }
}
