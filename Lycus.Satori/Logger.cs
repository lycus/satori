using System;
using System.Linq;

namespace Lycus.Satori
{
    public abstract class Logger : IDisposable
    {
        public const int MinLogLevel = 0;

        public const int MaxLogLevel = 3;

        int _level;

        public int Level
        {
            get { return _level; }
            set
            {
                if (value < MinLogLevel || value > MaxLogLevel)
                    throw new ArgumentOutOfRangeException("value", value,
                        "Log level is out of range.");

                _level = value;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~Logger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        static bool HaveLogLevel(params string[] levels)
        {
            return levels.Contains(Environment.GetEnvironmentVariable("SATORI_LOG"));
        }

        public void Verbose(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            if (_level >= 1 || !HaveLogLevel("verbose", "debug", "trace"))
                return;

            if (args.Length != 0)
                format = format.Interpolate(args);

            OnVerboseLog("[V] " + format);
        }

        public void VerboseCore(Core core, string format, params object[] args)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            Verbose("{0} {1}".Interpolate(core.Id, format), args);
        }

        public void Debug(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            if (_level >= 2 || !HaveLogLevel("debug", "trace"))
                return;

            if (args.Length != 0)
                format = format.Interpolate(args);

            OnDebugLog("[D] " + format);
        }

        public void DebugCore(Core core, string format, params object[] args)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            Debug("{0} {1}".Interpolate(core.Id, format), args);
        }

        public void Trace(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            if (_level >= 3 || !HaveLogLevel("trace"))
                return;

            if (args.Length != 0)
                format = format.Interpolate(args);

            OnTraceLog("[T] " + format);
        }

        public void TraceCore(Core core, string format, params object[] args)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            Trace("{0} {1}".Interpolate(core.Id, format), args);
        }

        protected abstract void OnVerboseLog(string message);

        protected abstract void OnDebugLog(string message);

        protected abstract void OnTraceLog(string message);
    }
}
