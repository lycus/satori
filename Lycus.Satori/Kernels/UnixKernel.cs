using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

// HACK: To make compilation work with MCS.
#pragma warning disable 0219
#pragma warning disable 0414

namespace Lycus.Satori.Kernels
{
    public sealed class UnixKernel : Kernel
    {
        const int ErrorNoEntry = 2;

        const int ErrorIO = 5;

        const int ErrorBadFile = 9;

        const int ErrorNoAccess = 13;

        const int ErrorMemoryFault = 14;

        const int ErrorExists = 17;

        const int ErrorNameTooLong = 36;

        const int ErrorNoSystem = 38;

        const int MaxPath = 4096;

        public override Capabilities Capabilities
        {
            get { return Capabilities.All; }
        }

        bool _disposed;

        readonly ConcurrentQueue<int> _handles = new ConcurrentQueue<int>();

        readonly ConcurrentDictionary<int, Stream> _files = new ConcurrentDictionary<int, Stream>();

        volatile int _lastHandle;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public UnixKernel()
        {
            _files.TryAdd(_lastHandle++, Console.OpenStandardInput());
            _files.TryAdd(_lastHandle++, Console.OpenStandardOutput());
            _files.TryAdd(_lastHandle++, Console.OpenStandardError());
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                foreach (var stream in _files.Values)
                {
                    stream.Flush();
                    stream.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override void SystemCall(Core core, ref int r3, ref int r0, int r1, int r2)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var code = r3;

            r3 = SystemCallSuccess;
            r0 = 0;

            // These system calls are numbered and implemented according to
            // the system calls in the Epiphany port of Newlib, which are
            // also used by the official, CGEN-based Epiphany simulator.
            switch (code)
            {
                // open(path, flags, mode)
                case 2:
                    HandleOpen(core, ref r3, ref r0, r1, r2);
                    break;
                // close(file)
                case 3:
                    HandleClose(ref r3, ref r0);
                    break;
                // read(file, buffer, length)
                case 4:
                    break;
                // write(file, buffer, length)
                case 5:
                    break;
                // lseek(file, offset, whence)
                case 6:
                    break;
                // unlink(path)
                case 7:
                    break;
                // fstat(file, stat)
                case 10:
                    break;
                // stat(path, stat)
                case 15:
                    break;
                // gettimeofday(time, zone)
                case 19:
                    break;
                // link(target, name)
                case 21:
                    break;
                default:
                    r3 = ErrorNoSystem;
                    break;
            }
        }

        void HandleOpen(Core core, ref int r3, ref int r0, int r1, int r2)
        {
            var path = new byte[MaxPath];
            var addr = (uint)r0;

            if (addr == 0)
            {
                r3 = ErrorMemoryFault;

                return;
            }

            for (uint i = 0; i < MaxPath + 1; i++)
            {
                if (i == MaxPath)
                {
                    r3 = ErrorNameTooLong;

                    return;
                }

                var chr = core.Machine.Memory.ReadByte(core, addr + i);

                if (chr == 0)
                {
                    var len = (int)i + 1;
                    var path2 = new byte[len];

                    Buffer.BlockCopy(path, 0, path2, 0, len);

                    path = path2;

                    break;
                }

                path[i] = chr;
            }

            var realPath = Encoding.ASCII.GetString(path);

            FileStream stream;

            // Attempt to translate possible errors to `errno` values.
            try
            {
                FileMode mode = 0;

                if ((r1 & 0x00000008) != 0)
                    mode = FileMode.Append;

                var access = FileAccess.Read;

                if ((r1 & 0x00000001) != 0)
                    access = FileAccess.Write;

                if ((r1 & 0x00000002) != 0)
                    access = FileAccess.ReadWrite;

                var share = FileShare.ReadWrite | FileShare.Inheritable | FileShare.Delete;

                stream = File.Open(realPath, mode, access, share);
            }
            catch (ArgumentException)
            {
                // Interestingly, Linux just treats an invalid path as not
                // existing on the file system.
                r3 = ErrorNoEntry;

                return;
            }
            catch (PathTooLongException)
            {
                r3 = ErrorNameTooLong;

                return;
            }
            catch (DirectoryNotFoundException)
            {
                r3 = ErrorNoEntry;

                return;
            }
            catch (UnauthorizedAccessException)
            {
                // This exception actually means a lot more than just lack
                // of permissions, but we can't disambiguate.
                r3 = ErrorNoAccess;

                return;
            }
            catch (FileNotFoundException)
            {
                r3 = ErrorNoEntry;

                return;
            }
            catch (NotSupportedException)
            {
                r3 = ErrorNoEntry;

                return;
            }
            catch (IOException)
            {
                // Unfortunately, we can't do better.
                r3 = ErrorIO;

                return;
            }
        }

        void HandleClose(ref int r3, ref int r0, int r1, int r2)
        {
            Stream stream;

            if (_files.TryRemove(r0, out stream))
                stream.Dispose();
            else
                r3 = ErrorBadFile;
        }

        void HandleClose(ref int r3, ref int r0)
        {
        }

        void HandleRead(ref int r3, ref int r0, int r1, int r2)
        {
        }

        void HandleWrite(ref int r3, ref int r0, int r1, int r2)
        {
        }

        void HandleSeek(ref int r3, ref int r0, int r1, int r2)
        {
        }

        void HandleUnlink(ref int r3, ref int r0)
        {
        }

        void HandleFileStat(ref int r3, ref int r0, int r1)
        {
        }

        void HandleStat(ref int r3, ref int r0, int r1)
        {
        }

        void HandleGetTimeOfDay(ref int r3, ref int r0, int r1)
        {
        }

        void HandleLink(ref int r3, ref int r0, int r1)
        {
        }
    }
}
