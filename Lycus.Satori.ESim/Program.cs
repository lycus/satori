using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Lycus.Satori;
using Lycus.Satori.Kernels;
using Lycus.Satori.Loggers;

namespace Lycus.Satori.ESim
{
    static class Program
    {
        static Machine _machine;

        static int ParseInt32Variable(string variable, int fallback)
        {
            var value = Environment.GetEnvironmentVariable(variable);

            if (value == null)
                return fallback;

            int result;

            if (int.TryParse(value, out result))
                return result;

            return fallback;
        }

        static T ParseEnumVariable<T>(string variable, T fallback)
            where T : struct
        {
            var value = Environment.GetEnvironmentVariable(variable);

            if (value == null)
                return fallback;

            T result;

            if (Enum.TryParse<T>(value, out result))
                return result;

            return fallback;
        }

        static string GetStringVariable(string variable, string fallback)
        {
            var value = Environment.GetEnvironmentVariable(variable);

            return value != null ? value : fallback;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static int RealMain(string[] args)
        {
            var r = ParseInt32Variable("E_SIM_ROWS", 8);
            var c = ParseInt32Variable("E_SIM_COLS", 8);

            if (r < Machine.MinRows || r > Machine.MaxRows ||
                c < Machine.MinColumns || c > Machine.MaxColumns)
            {
                Console.Error.WriteLine("Invalid E_SIM_ROWS * E_SIM_COLS values given");
                return 1;
            }

            if (new CoreId(r - 1, c - 1).ToAddress() >= Memory.ExternalBaseAddress)
            {
                Console.Error.WriteLine("A E_SIM_ROWS * E_SIM_COLS grid would overlap external memory");
                return 1;
            }

            var m = ParseInt32Variable("E_SIM_MLEN", Memory.MinMemorySize);

            if (m < Memory.MinMemorySize || m > Memory.MaxMemorySize)
            {
                Console.Error.WriteLine("Invalid E_SIM_MLEN value given");
                return 1;
            }

            var k = GetStringVariable("E_SIM_KERN", "unix");

            Kernel kern;

            switch (k)
            {
                case "null":
                    kern = new NullKernel();
                    break;
                case "unix":
                    kern = new UnixKernel();
                    break;
                default:
                    Console.Error.WriteLine("Invalid E_SIM_KERN value given");
                    return 1;
            }

            var arch = ParseEnumVariable<Architecture>("E_SIM_ARCH", Architecture.EpiphanyIV);

            _machine = new Machine(arch, new ConsoleLogger(), kern, r, c, m);

            var cores = new List<Core>();

            while (args.Length != 0)
            {
                // If we encounter `--`, we pass all remaining arguments
                // down to the program being simulated.
                if (args[0] == "--")
                {
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

                if (row < 0 || row >= r ||
                    column < 0 || column >= c)
                {
                    Console.Error.WriteLine("Out-of-bounds row/column numbers given for {0}", file);
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

            // Wait for cores to stop normally.
            _machine.Join();

            _machine.Dispose();
            _machine = null;

            return 0;
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
