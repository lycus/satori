using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Lycus.Satori
{
    public sealed class Memory
    {
        [CLSCompliant(false)]
        public const uint InterruptVectorAddress = 0x00000000;

        public const int InterruptVectorSize = sizeof(uint) * 16;

        [CLSCompliant(false)]
        public const uint LocalBank0Address = 0x00000040;

        public const int LocalBank0Size = 8 * 1024 - sizeof(uint) * 16;

        [CLSCompliant(false)]
        public const uint LocalBank1Address = 0x00002000;

        public const int LocalBank1Size = 8 * 1024;

        [CLSCompliant(false)]
        public const uint LocalBank2Address = 0x00004000;

        public const int LocalBank2Size = 8 * 1024;

        [CLSCompliant(false)]
        public const uint LocalBank3Address = 0x00006000;

        public const int LocalBank3Size = 8 * 1024;

        [CLSCompliant(false)]
        public const uint Reserved0Address = 0x00008000;

        public const int Reserved0Size = 928 * 1024;

        [CLSCompliant(false)]
        public const uint RegisterFileAddress = 0x000F0000;

        public const int RegisterFileSize = 2 * 1024;

        [CLSCompliant(false)]
        public const uint Reserved1Address = 0x000F0800;

        public const int Reserved1Size = 62 * 1024;

        public const int LocalMemorySize = 1024 * 1024;

        [CLSCompliant(false)]
        public const uint ExternalBaseAddress = 0x8E000000;

        public const int MinMemorySize = 32 * 1024 * 1024;

        public const int MaxMemorySize = 1024 * 1024 * 1024;

        public Machine Machine { get; private set; }

        public int Size { get; private set; }

        /// <summary>
        /// The global memory lock.
        ///
        /// This is used when memory accesses to external memory
        /// happen. Callers can lock on this object to perform
        /// atomic operations on external memory.
        /// </summary>
        public object Lock { get; private set; }

        readonly byte[] _external;

        internal Memory(Machine machine, int size)
        {
            Machine = machine;
            Size = size;
            Lock = new object();
            _external = new byte[size];
        }

        [CLSCompliant(false)]
        public uint Translate(Core core, uint address,
            out Core target, out byte[] memory, out object @lock)
        {
            return Translate(core, address, null,
                out target, out memory, out @lock);
        }

        uint Translate(Core core, uint address, bool? write,
            out Core target, out byte[] memory, out object @lock)
        {
            // Is it in external memory?
            if (address >= ExternalBaseAddress && address < ExternalBaseAddress + Size)
            {
                target = null;
                memory = _external;
                @lock = Lock;

                return address - ExternalBaseAddress;
            }

            uint raw;

            var id = CoreId.FromAddress(address, out raw);

            // These ranges are reserved for future expansion of
            // local core memory by Adapteva.
            if (raw >= Reserved0Address && raw < RegisterFileAddress ||
                raw >= Reserved1Address && raw < 0x10000)
            {
                if (write != null)
                    throw new MemoryException(
                        "Reserved memory access at 0x{0:X8}.".Interpolate(address),
                        address, (bool)write);

                target = null;
                memory = null;
                @lock = null;

                return uint.MaxValue;
            }

            if (id != CoreId.Current)
                core = Machine.GetCore(id);

            // Is it local core memory?
            if (raw < LocalMemorySize && core != null)
            {
                target = core;
                memory = core.Memory;
                @lock = core.Lock;

                return raw;
            }

            // No? Then it's trying to access arbitrary memory, at
            // which point we have to bail since we can't give it
            // access to anything meaningful. Even on a real board,
            // accessing arbitrary physical memory is questionable
            // at best, since it can screw the host kernel up.
            if (write != null)
                throw new MemoryException(
                    "Invalid memory access at 0x{0:X8}.".Interpolate(address),
                    address, (bool)write);

            target = null;
            memory = null;
            @lock = null;

            return address;
        }

        unsafe void RawWrite(Core writer, uint address, void* data, int size)
        {
            Core tgt;
            byte[] mem;
            object @lock;

            var idx = Translate(writer, address, true, out tgt, out mem, out @lock);

            if (idx + size >= mem.Length)
                throw new MemoryException(
                    "Out-of-bounds memory access at 0x{0:X8}.".Interpolate(address),
                    address, true);

            lock (@lock)
                Marshal.Copy(new IntPtr(data), mem, (int)idx, size);
        }

        unsafe void RawRead(Core reader, uint address, void* data, int size)
        {
            Core tgt;
            byte[] mem;
            object @lock;

            var idx = Translate(reader, address, false, out tgt, out mem, out @lock);

            if (idx + size >= mem.Length)
                throw new MemoryException(
                    "Out-of-bounds memory access at 0x{0:X8}.".Interpolate(address),
                    address, false);

            lock (@lock)
                Marshal.Copy(mem, (int)idx, new IntPtr(data), size);
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, sbyte value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            Write(writer, address, (byte)value);
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, byte value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            RawWrite(writer, address, &value, sizeof(byte));
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, short value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            Write(writer, address, (ushort)value);
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, ushort value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            RawWrite(writer, address, &value, sizeof(ushort));
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, int value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            Write(writer, address, (uint)value);
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, uint value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            RawWrite(writer, address, &value, sizeof(uint));
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, long value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            Write(writer, address, (ulong)value);
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, ulong value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            RawWrite(writer, address, &value, sizeof(ulong));
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, float value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            Write(writer, address, value.CoerceToUInt32());
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, double value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            Write(writer, address, value.CoerceToUInt64());
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, byte[] data)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (data == null)
                throw new ArgumentNullException("data");

            fixed (byte* p = &data[0])
                RawWrite(writer, address, p, data.Length);
        }

        [CLSCompliant(false)]
        public sbyte ReadSByte(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return (sbyte)ReadByte(reader, address);
        }

        [CLSCompliant(false)]
        public unsafe byte ReadByte(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            byte result;

            RawRead(reader, address, &result, sizeof(byte));

            return result;
        }

        [CLSCompliant(false)]
        public short ReadInt16(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return (short)ReadUInt16(reader, address);
        }

        [CLSCompliant(false)]
        public unsafe ushort ReadUInt16(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            ushort result;

            RawRead(reader, address, &result, sizeof(ushort));

            return result;
        }

        [CLSCompliant(false)]
        public int ReadInt32(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return (int)ReadUInt32(reader, address);
        }

        [CLSCompliant(false)]
        public unsafe uint ReadUInt32(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            uint result;

            RawRead(reader, address, &result, sizeof(uint));

            return result;
        }

        [CLSCompliant(false)]
        public long ReadInt64(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return (long)ReadUInt64(reader, address);
        }

        [CLSCompliant(false)]
        public unsafe ulong ReadUInt64(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            ulong result;

            RawRead(reader, address, &result, sizeof(ulong));

            return result;
        }

        [CLSCompliant(false)]
        public float ReadSingle(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return ReadUInt32(reader, address).CoerceToSingle();
        }

        [CLSCompliant(false)]
        public double ReadDouble(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return ReadUInt64(reader, address).CoerceToDouble();
        }

        [CLSCompliant(false)]
        public unsafe byte[] ReadBytes(Core reader, uint address, int count)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (count < 0)
                throw new ArgumentOutOfRangeException(
                    "count",
                    "Byte count is negative.");

            var data = new byte[count];

            fixed (byte* p = &data[0])
                RawRead(reader, address, p, count);

            return data;
        }
    }
}
