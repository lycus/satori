using System;
using System.Runtime.InteropServices;

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

        internal bool AlignmentChecks { get; set; }

        readonly byte[] _external;

        internal Memory(Machine machine, int size)
        {
            Machine = machine;
            Size = size;
            Lock = new object();
            AlignmentChecks = true;
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

        bool CheckRegisterAccess(byte[] bank, uint address,
            uint index, int size, bool write)
        {
            if (bank == _external)
                return false;

            if (index < RegisterFileAddress || index >= RegisterFileAddress + RegisterFileSize)
                return false;

            if (address % sizeof(uint) != 0)
                throw new MemoryException(
                    "Unaligned register access at 0x{1:X8}.".Interpolate(address),
                    address, write);

            if (size != sizeof(uint))
                throw new MemoryException(
                    "Invalid register size {0} at 0x{1:X8}.".Interpolate(size, address),
                    address, write);

            if (index >= 0xF00FC && index < 0xF0400 ||
                index == 0xF0410 ||
                index == 0xF0444 ||
                index >= 0xF044C && index < 0xF0500 ||
                index >= 0xF0540 && index < 0xF0604 ||
                index >= 0xF060C && index < 0xF0700 ||
                index >= 0xF071C && index < 0xF0800)
                throw new MemoryException(
                    "Reserved register access at 0x{0:X8}.".Interpolate(address),
                    address, write);

            return true;
        }

        static unsafe void HandleRegisterWrite(Core core, uint index, uint* value)
        {
            switch (index)
            {
                case RegisterFile.CoreStatusAddress:
                    // Never write the first three bits; instead,
                    // preserve the ones already set.
                    *value = Bits.Insert(*value, Bits.Extract(core.Registers.CoreStatus, 0, 3), 0, 3);

                    break;
                case RegisterFile.CoreStatusStoreAddress:
                    // Have to bypass the regular memory access
                    // machinery since otherwise the first three
                    // bits are masked out.
                    LowWrite(value, core.Memory, RegisterFile.CoreStatusAddress, sizeof(int));

                    break;
                case RegisterFile.DebugCommandAddress:
                    var cmd = Bits.Extract(*value, 0, 2);

                    switch (cmd)
                    {
                        case 0x0:
                            // TODO: Clear `MBKPT_FLAG` too?
                            core.Registers.DebugStatus = Bits.Clear(core.Registers.DebugStatus, 0);
                            break;
                        case 0x1:
                            core.Registers.DebugStatus = Bits.Set(core.Registers.DebugStatus, 0);
                            break;
                    }

                    break;
                case RegisterFile.ResetCoreAddress:
                    // Zero all registers. Technically, we don't
                    // have to zero all of them, but it's easier.
                    if (!Bits.Check(*value, 0))
                        Array.Clear(core.Memory, (int)RegisterFileAddress, (int)RegisterFileSize);

                    // TODO: Should we set `DEBUGSTATUS`?

                    break;
                case RegisterFile.InterruptLatchStoreAddress:
                    var ilatst = core.Registers.InterruptLatch;

                    foreach (Interrupt val in Enum.GetValues(typeof(Interrupt)))
                        if (Bits.Check(*value, (int)val))
                            ilatst = Bits.Set(ilatst, (int)val);

                    core.Registers.InterruptLatch = ilatst;

                    break;
                case RegisterFile.InterruptLatchClearAddress:
                    var ilatcl = core.Registers.InterruptLatch;

                    foreach (Interrupt val in Enum.GetValues(typeof(Interrupt)))
                        if (Bits.Check(*value, (int)val))
                            ilatcl = Bits.Clear(ilatcl, (int)val);

                    core.Registers.InterruptLatch = ilatcl;

                    break;
            }
        }

        static unsafe void LowWrite(void* data, byte[] memory, uint index, int size)
        {
            Marshal.Copy(new IntPtr(data), memory, (int)index, size);
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

            var reg = CheckRegisterAccess(mem, address, idx, size, true);

            if (!reg)
                Machine.Logger.TraceCore(writer, "Writing {0} bytes to 0x{1:X8}",
                    size, address);

            lock (@lock)
            {
                if (reg)
                    HandleRegisterWrite(tgt, idx, (uint*)data);

                LowWrite(data, mem, idx, size);
            }
        }

        static unsafe void LowRead(byte[] memory, uint index, void* data, int size)
        {
            Marshal.Copy(memory, (int)index, new IntPtr(data), size);
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

            if (!CheckRegisterAccess(mem, address, idx, size, false))
                Machine.Logger.TraceCore(reader, "Reading {0} bytes from 0x{1:X8}",
                    size, address);

            lock (@lock)
                LowRead(mem, idx, data, size);
        }

        void CheckAlignment(uint address, int size)
        {
            if (AlignmentChecks && address % size != 0)
                throw new DataMisalignedException(
                    "Unaligned memory access of size {0} at 0x{1:X8}.".Interpolate(size, address));
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

            CheckAlignment(address, sizeof(short));

            Write(writer, address, (ushort)value);
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, ushort value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            CheckAlignment(address, sizeof(ushort));

            RawWrite(writer, address, &value, sizeof(ushort));
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, int value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            CheckAlignment(address, sizeof(int));

            Write(writer, address, (uint)value);
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, uint value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            CheckAlignment(address, sizeof(uint));

            RawWrite(writer, address, &value, sizeof(uint));
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, long value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            CheckAlignment(address, sizeof(long));

            Write(writer, address, (ulong)value);
        }

        [CLSCompliant(false)]
        public unsafe void Write(Core writer, uint address, ulong value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            CheckAlignment(address, sizeof(ulong));

            RawWrite(writer, address, &value, sizeof(ulong));
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, float value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            CheckAlignment(address, sizeof(float));

            Write(writer, address, value.CoerceToUInt32());
        }

        [CLSCompliant(false)]
        public void Write(Core writer, uint address, double value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            CheckAlignment(address, sizeof(double));

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

            CheckAlignment(address, sizeof(short));

            return (short)ReadUInt16(reader, address);
        }

        [CLSCompliant(false)]
        public unsafe ushort ReadUInt16(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            CheckAlignment(address, sizeof(ushort));

            ushort result;

            RawRead(reader, address, &result, sizeof(ushort));

            return result;
        }

        [CLSCompliant(false)]
        public int ReadInt32(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            CheckAlignment(address, sizeof(int));

            return (int)ReadUInt32(reader, address);
        }

        [CLSCompliant(false)]
        public unsafe uint ReadUInt32(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            CheckAlignment(address, sizeof(uint));

            uint result;

            RawRead(reader, address, &result, sizeof(uint));

            return result;
        }

        [CLSCompliant(false)]
        public long ReadInt64(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            CheckAlignment(address, sizeof(long));

            return (long)ReadUInt64(reader, address);
        }

        [CLSCompliant(false)]
        public unsafe ulong ReadUInt64(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            CheckAlignment(address, sizeof(ulong));

            ulong result;

            RawRead(reader, address, &result, sizeof(ulong));

            return result;
        }

        [CLSCompliant(false)]
        public float ReadSingle(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            CheckAlignment(address, sizeof(float));

            return ReadUInt32(reader, address).CoerceToSingle();
        }

        [CLSCompliant(false)]
        public double ReadDouble(Core reader, uint address)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            CheckAlignment(address, sizeof(double));

            return ReadUInt64(reader, address).CoerceToDouble();
        }

        [CLSCompliant(false)]
        public unsafe byte[] ReadBytes(Core reader, uint address, int count)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                    "Byte count is negative.");

            var data = new byte[count];

            fixed (byte* p = &data[0])
                RawRead(reader, address, p, count);

            return data;
        }
    }
}
