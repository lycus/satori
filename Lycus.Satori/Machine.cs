using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lycus.Satori
{
    /// <summary>
    /// Represents an Epiphany platform.
    /// </summary>
    public sealed class Machine : IDisposable
    {
        public const int MinRows = 2;

        public const int MaxRows = 64;

        public const int MinColumns = 2;

        public const int MaxColumns = 64;

        /// <summary>
        /// The architecture version of this machine.
        /// </summary>
        public Architecture Architecture { get; private set; }

        /// <summary>
        /// The logger in use by this machine.
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        /// The kernel in use by this machine.
        /// </summary>
        public Kernel Kernel { get; private set; }

        /// <summary>
        /// The memory system of this machine.
        /// </summary>
        public Memory Memory { get; private set; }

        /// <summary>
        /// The instruction fetcher used by this machine.
        /// </summary>
        public Fetcher Fetcher { get; private set; }

        public int Rows { get; private set; }

        public int Columns { get; private set; }

        public IReadOnlyList<Core> Cores { get; private set; }

        public IReadOnlyList<IReadOnlyList<Core>> Grid { get; private set; }

        public TimeSpan SleepDuration { get; set; }

        public TimeSpan IdleDuration { get; set; }

        internal bool Halting { get; private set; }

        bool _disposed;

        public Machine(Architecture architecture, Logger logger, Kernel kernel,
            int rows, int columns, int memory)
        {
            if (architecture != Architecture.EpiphanyIII &&
                architecture != Architecture.EpiphanyIV)
                throw new ArgumentException(
                    "Invalid Architecture value ({0}).".Interpolate(architecture),
                    "architecture");

            if (logger == null)
                throw new ArgumentNullException("logger");

            if (kernel == null)
                throw new ArgumentNullException("kernel");

            if (rows < MinRows || rows > MaxRows)
                throw new ArgumentOutOfRangeException("rows", rows,
                    "Row count is out of range.");

            if (columns < MinColumns || columns > MaxColumns)
                throw new ArgumentOutOfRangeException("columns", columns,
                    "Column count is out of range.");

            if (new CoreId(rows - 1, columns - 1).ToAddress() >= Memory.ExternalBaseAddress)
                throw new ArgumentException("The given row and column counts would result " +
                    "in a grid that overlaps external memory.");

            if (memory < Memory.MinMemorySize || memory > Memory.MaxMemorySize)
                throw new ArgumentOutOfRangeException("memory", memory,
                    "External memory size is out of range.");

            Architecture = architecture;
            Logger = logger;
            Kernel = kernel;
            Memory = new Memory(this, memory);
            Fetcher = new Fetcher(this);
            Rows = rows;
            Columns = columns;

            var cores = new List<Core>(rows * columns);
            var grid = new List<List<Core>>(rows);

            for (var i = 0; i < rows; i++)
            {
                var row = new List<Core>(columns);

                for (var j = 0; j < columns; j++)
                    row.Add(new Core(this, new CoreId(i, j)));

                cores.AddRange(row);
                grid.Add(row);
            }

            Cores = cores;
            Grid = grid;
            SleepDuration = TimeSpan.FromMilliseconds(50);
            IdleDuration = TimeSpan.FromMilliseconds(10);
        }

        public Core GetCore(CoreId id)
        {
            if (id.Row >= Grid.Count)
                return null;

            var list = Grid[id.Row];

            return id.Column >= list.Count ? null : list[id.Column];
        }

        public void Join()
        {
            Task.WaitAll(Cores.Select(x => x.MainTask).ToArray());
        }

        void RealDispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Flag all cores for shutdown.
            Halting = true;

            // Wait for all cores to shut down.
            Join();

            // Clean up the rest now that nothing will call it.
            Kernel.Dispose();
            Logger.Dispose();
            Fetcher.Dispose();
        }

        ~Machine()
        {
            RealDispose();
        }

        public void Dispose()
        {
            RealDispose();

            GC.SuppressFinalize(this);
        }
    }
}
