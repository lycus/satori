using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Lycus.Satori.Kernels;
using Lycus.Satori.Loggers;
using Mono.Options;

namespace Lycus.Satori.EExec
{
    static class Program
    {
        static Machine _machine;

        static void ShowHelp(OptionSet set)
        {
            Console.WriteLine("This is e-exec, part of Satori.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("  e-exec [options]... {<file> <row> <column>}...");
            Console.WriteLine("  e-exec [options]... program.elf 4 6");
            Console.WriteLine("  e-exec [options]... prog1.elf 1 1 prog2.elf 1 2");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine();

            set.WriteOptionDescriptions(Console.Out);

            Console.WriteLine();
            Console.WriteLine("Min:max rows: {0}:{1}", Machine.MinRows, Machine.MaxRows);
            Console.WriteLine("Min:max columns: {0}:{1}", Machine.MinColumns, Machine.MaxColumns);
            Console.WriteLine("Min:max memory: 0x{0:X8}:0x{1:X8}", Memory.MinMemorySize, Memory.MaxMemorySize);
            Console.WriteLine();
            Console.WriteLine("Valid architecture values:");
            Console.WriteLine();

            foreach (var val in Enum.GetNames(typeof(Architecture)))
                Console.WriteLine("  " + val);

            Console.WriteLine();
            Console.WriteLine("Valid kernel values:");
            Console.WriteLine();
            Console.WriteLine("  none: No host kernel.");
            Console.WriteLine("  unix: Unix-like kernel.");
            Console.WriteLine();
            Console.WriteLine("All non-option arguments are interpreted as <file> <row> <column>");
            Console.WriteLine("specifications, stating which ELF files to load to which core.");
            Console.WriteLine();
        }

        static void ShowVersion()
        {
            Console.WriteLine("Satori e-exec " + typeof(Program).Assembly.GetName().Version);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static int RealMain(string[] args)
        {
            var version = false;
            var help = false;
            var plat = false;
            var rows = 8;
            var cols = 8;
            var emem = Memory.MinMemorySize;
            var arch = Architecture.EpiphanyIV;
            var kern = "unix";

            var set = new OptionSet
            {
                { "v|version", "Show version information and exit.", v => version = v != null },
                { "h|help", "Show this help message and exit.", v => help = v != null },
                { "p|platform", "Use the hardware platform.", v => plat = v != null },
                { "r|rows=", "Specify rows in the grid.", (int v) => rows = v },
                { "c|columns=", "Specify columns in the grid.", (int v) => cols = v },
                { "m|memory=", "Specify external memory segment size.", (int v) => emem = v },
                { "a|architecture=", "Specify Epiphany architecture version.", (Architecture v) => arch = v },
                { "k|kernel=", "Specify host-side kernel.", v => kern = v },
            };

            args = set.Parse(args).ToArray();

            if (version)
            {
                ShowVersion();
                return 0;
            }

            if (help)
            {
                ShowHelp(set);
                return 0;
            }

            if (plat)
            {
                Console.Error.WriteLine("The hardware platform is not currently supported");
                return 1;
            }

            if (rows < Machine.MinRows || rows > Machine.MaxRows ||
                cols < Machine.MinColumns || cols > Machine.MaxColumns)
            {
                Console.Error.WriteLine("{0} * {1} grid specification is invalid", rows, cols);
                return 1;
            }

            if (new CoreId(rows - 1, cols - 1).ToAddress() >= Memory.ExternalBaseAddress)
            {
                Console.Error.WriteLine("A {0} * {1} grid would overlap external memory", rows, cols);
                return 1;
            }

            if (emem < Memory.MinMemorySize || emem > Memory.MaxMemorySize)
            {
                Console.Error.WriteLine("External memory segment size {0} is invalid", emem);
                return 1;
            }

            Kernel kernel;

            switch (kern)
            {
                case "null":
                    kernel = new NullKernel();
                    break;
                case "unix":
                    kernel = new UnixKernel();
                    break;
                default:
                    Console.Error.WriteLine("{0} is not a known kernel", kern);
                    return 1;
            }

            _machine = new Machine(arch, new ConsoleLogger(), kernel, rows, cols, emem);

            var cores = new List<Core>();

            while (args.Length != 0)
            {
                // If we encounter `--`, we pass all remaining arguments
                // down to the program being simulated.
                if (args[0] == "--")
                {
                    // TODO: Actually use this.
                    args = args.Skip(1).ToArray();
                    break;
                }

                if (args.Length < 3)
                {
                    Console.Error.WriteLine("Expected <file> <row> <column> arguments");
                    return 1;
                }

                var file = args[0];

                int row;
                int column;

                if (!int.TryParse(args[1], out row) ||
                    !int.TryParse(args[2], out column))
                {
                    Console.Error.WriteLine("Invalid row/column numbers given for {0}", file);
                    return 1;
                }

                if (row < 0 || row >= rows ||
                    column < 0 || column >= cols)
                {
                    Console.Error.WriteLine("Coordinates {0} * {1} for {2} are invalid", row, column, file);
                    return 1;
                }

                using (var stream = File.OpenRead(file))
                {
                    var core = _machine.GetCore(new CoreId(row, column));

                    Loader.LoadCode(core, stream);

                    cores.Add(core);
                }

                args = args.Skip(3).ToArray();
            }

            // Set all cores to active.
            foreach (var core in cores)
                core.Registers.CoreStatus |= 1 << 0;

            while (cores.Any(x => (x.Registers.CoreStatus & 1 << 0) == 1))
                Thread.Sleep(10);

            return cores.Any(x => x.TestFailed) ? 1 : 0;
        }

        static int Main(string[] args)
        {
            try
            {
                return RealMain(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("{0}: {1}", ex.GetType(), ex.Message);

                return 1;
            }
            finally
            {
                if (_machine != null)
                    _machine.Dispose();
            }
        }
    }
}
