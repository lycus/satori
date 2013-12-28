using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lycus.Satori.Instructions;

namespace Lycus.Satori
{
    public sealed class Fetcher : IDisposable
    {
        public Machine Machine { get; private set; }

        readonly HashSet<Func<uint, bool, Instruction>> _fetchers;

        readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        bool _disposed;

        public Fetcher(Machine machine)
        {
            Machine = machine;
            _fetchers = new HashSet<Func<uint, bool, Instruction>>();
        }

        void RealDispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _lock.Dispose();
        }

        ~Fetcher()
        {
            RealDispose();
        }

        public void Dispose()
        {
            RealDispose();

            GC.SuppressFinalize(this);
        }

        [CLSCompliant(false)]
        public bool AddFetcher(Func<uint, bool, Instruction> fetcher)
        {
            if (fetcher == null)
                throw new ArgumentNullException("fetcher");

            _lock.EnterWriteLock();

            var res = _fetchers.Add(fetcher);

            _lock.ExitWriteLock();

            return res;
        }

        [CLSCompliant(false)]
        public bool RemoveFetcher(Func<uint, bool, Instruction> fetcher)
        {
            if (fetcher == null)
                throw new ArgumentNullException("fetcher");

            _lock.EnterWriteLock();

            var res = _fetchers.Remove(fetcher);

            _lock.ExitWriteLock();

            return res;
        }

        public void ClearFetchers()
        {
            _lock.EnterWriteLock();

            _fetchers.Clear();

            _lock.ExitWriteLock();
        }

        public Func<uint, bool, Instruction>[] GetFetchers()
        {
            _lock.EnterReadLock();

            var arr = _fetchers.ToArray();

            _lock.ExitReadLock();

            return arr;
        }

        [CLSCompliant(false)]
        public Instruction Fetch16Bit(ushort value)
        {
            // And now for some voodoo magic. What we do here is check all
            // of the constant parts of the instruction according to the
            // decode table in the architecture reference. We try very hard
            // to match the entire instruction (even if just parts would be
            // enough) since that's more future-proof.

            var bits = value.ToBitArray();

            // First match the instruction prefix. This roughly classifies
            // instructions and lets us poke at a few specific bits when
            // necessary.
            switch (value & ~0xFFF0)
            {
                case 0x0000:
                    return new BranchInstruction(value, true);
                case 0x0001:
                    if (bits[4])
                        return new StoreIndexInstruction(value, true);

                    return new LoadIndexInstruction(value, true);
                case 0x0002:
                    switch ((value & ~0xFC0F) >> 4)
                    {
                        case 0x0018:
                            return new WiredAndInstruction(value);
                        case 0x0019:
                            return new GlobalInterruptEnableInstruction(value);
                        case 0x001A:
                            return new NoOperationInstruction(value);
                        case 0x001B:
                            return new IdleInstruction(value);
                        case 0x001C:
                            return new BreakpointInstruction(value);
                        case 0x001D:
                            return new ReturnInterruptInstruction(value);
                        case 0x001F:
                            return new SynchronizeInstruction(value);
                        case 0x0039:
                            return new GlobalInterruptDisableInstruction(value);
                        case 0x003C:
                            return new MulticoreBreakpointInstruction(value);
                        case 0x003E:
                            return new TrapInstruction(value);
                    }

                    if (!bits[8] && !bits[9])
                        return new MoveInstruction(value, true);

                    if (bits[8] && !bits[9])
                    {
                        switch ((value & ~0xFF0F) >> 4)
                        {
                            case 0x0000:
                                return new MoveToSystemInstruction(value, true);
                            case 0x0001:
                                return new MoveFromSystemInstruction(value, true);
                            case 0x0004:
                                return new JumpInstruction(value, true);
                            case 0x0005:
                                return new JumpLinkInstruction(value, true);
                        }
                    }

                    break;
                case 0x0004:
                    if (bits[4])
                        return new StoreDisplacementInstruction(value, true);

                    return new LoadDisplacementInstruction(value, true);
                case 0x0005:
                    if (bits[4])
                        return new StoreDisplacementPostModifyInstruction(value, true);

                    return new LoadDisplacementPostModifyInstruction(value, true);
                case 0x0006:
                    if (bits[4])
                        return new LogicalShiftLeftImmediateInstruction(value, true);

                    return new LogicalShiftRightImmediateInstruction(value, true);
                case 0x0007:
                    switch ((value & ~0xFF8F) >> 4)
                    {
                        case 0x0000:
                            return new FloatAddInstruction(value, true);
                        case 0x0001:
                            return new FloatSubtractInstruction(value, true);
                        case 0x0002:
                            return new FloatMultiplyInstruction(value, true);
                        case 0x0003:
                            return new FloatMultiplyAddInstruction(value, true);
                        case 0x0004:
                            return new FloatMultiplySubtractInstruction(value, true);
                        case 0x0005:
                            return new FloatInstruction(value, true);
                        case 0x0006:
                            return new FixInstruction(value, true);
                        case 0x0007:
                            return new FloatAbsoluteInstruction(value, true);
                    }

                    break;
                case 0x0003:
                    if (!bits[4])
                        return new MoveImmediateInstruction(value, true);

                    if (bits[4] && bits[5] && !bits[6])
                        return new SubtractImmediateInstruction(value, true);

                    break;
                case 0x000A:
                    switch ((value & ~0xFF8F) >> 4)
                    {
                        case 0x0000:
                            return new ExclusiveOrInstruction(value, true);
                        case 0x0001:
                            return new AddInstruction(value, true);
                        case 0x0002:
                            return new LogicalShiftLeftInstruction(value, true);
                        case 0x0003:
                            return new SubtractInstruction(value, true);
                        case 0x0004:
                            return new LogicalShiftRightInstruction(value, true);
                        case 0x0005:
                            return new AndInstruction(value, true);
                        case 0x0006:
                            return new ArithmeticShiftRightInstruction(value, true);
                        case 0x0007:
                            return new OrInstruction(value, true);
                    }

                    break;
                case 0x000B:
                    if (bits[4] && !bits[5] && !bits[6])
                        return new AddImmediateInstruction(value, true);

                    break;
                case 0x000E:
                    if (bits[4])
                        return new BitReverseInstruction(value, true);

                    return new ArithmeticShiftRightImmediateInstruction(value, true);
            }

            // It's not an instruction we know about. Ask any extended
            // decoders whether they understand it.
            _lock.EnterReadLock();

            try
            {
                foreach (var insn in _fetchers.Select(dec => dec(value, true)).Where(insn => insn != null))
                    return insn;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Doesn't look like a valid 16-bit instruction.
            return null;
        }

        [CLSCompliant(false)]
        public Instruction Fetch32Bit(uint value)
        {
            // Same deal as with 16-bit instructions, except we care about
            // more constant bits in the instruction.

            var bits = value.ToBitArray();
            
            // First match the instruction prefix. This roughly classifies
            // instructions and lets us poke at a few specific bits when
            // necessary.
            switch (value & ~0xFFFFFFF0)
            {
                case 0x00000008:
                    return new BranchInstruction(value, false);
                case 0x00000009:
                    if (!bits[21] && !bits[22])
                    {
                        if (bits[4])
                            return new StoreIndexInstruction(value, false);

                        return new LoadIndexInstruction(value, false);
                    }

                    if (bits[21] && !bits[22])
                        return new TestSetInstruction(value);

                    break;
                case 0x0000000B:
                    if (!bits[4])
                    {
                        if (bits[28])
                            return new MoveHighImmediateInstruction(value);

                        return new MoveImmediateInstruction(value, false);
                    }

                    if (bits[4] && !bits[5] && !bits[6])
                        return new AddImmediateInstruction(value, false);

                    if (bits[4] && bits[5] && !bits[6])
                        return new SubtractImmediateInstruction(value, false);

                    break;
                case 0x0000000C:
                    if (bits[25])
                    {
                        if (bits[4])
                            return new StoreDisplacementPostModifyInstruction(value, false);

                        return new LoadDisplacementPostModifyInstruction(value, false);
                    }

                    if (bits[4])
                        return new StoreDisplacementInstruction(value, false);

                    return new LoadDisplacementInstruction(value, false);
                case 0x0000000D:
                    if (!bits[21] && !bits[22])
                    {
                        if (bits[4])
                            return new StorePostModifyInstruction(value);

                        return new LoadPostModifyInstruction(value);
                    }

                    break;
                case 0x0000000F:
                    switch ((value & ~0xFFF0FFFF) >> 16)
                    {
                        case 0x00000002:
                            if (!bits[8] && !bits[9])
                                return new MoveInstruction(value, false);

                            if (bits[8] && !bits[9])
                            {
                                switch ((value & ~0xFFFFFF0F) >> 4)
                                {
                                    case 0x00000000:
                                        return new MoveToSystemInstruction(value, false);
                                    case 0x00000001:
                                        return new MoveFromSystemInstruction(value, false);
                                    case 0x00000004:
                                        return new JumpInstruction(value, false);
                                    case 0x00000005:
                                        return new JumpLinkInstruction(value, false);
                                }
                            }

                            break;
                        case 0x00000006:
                            if (bits[4])
                                return new LogicalShiftLeftImmediateInstruction(value, false);

                            return new LogicalShiftRightImmediateInstruction(value, false);
                        case 0x00000007:
                            switch ((value & ~0xFFFFFF8F) >> 4)
                            {
                                case 0x00000000:
                                    return new FloatAddInstruction(value, false);
                                case 0x00000001:
                                    return new FloatSubtractInstruction(value, false);
                                case 0x00000002:
                                    return new FloatMultiplyInstruction(value, false);
                                case 0x00000003:
                                    return new FloatMultiplyAddInstruction(value, false);
                                case 0x00000004:
                                    return new FloatMultiplySubtractInstruction(value, false);
                                case 0x00000005:
                                    return new FloatInstruction(value, false);
                                case 0x00000006:
                                    return new FixInstruction(value, false);
                                case 0x00000007:
                                    return new FloatAbsoluteInstruction(value, false);
                            }

                            break;
                        case 0x0000000E:
                            if (bits[4])
                                return new BitReverseInstruction(value, false);

                            return new ArithmeticShiftRightImmediateInstruction(value, false);
                        case 0x0000000F:
                            return new UnimplementedInstruction(value);
                        case 0x0000000A:
                            switch ((value & ~0xFFFFFF8F) >> 4)
                            {
                                case 0x00000000:
                                    return new ExclusiveOrInstruction(value, false);
                                case 0x00000001:
                                    return new AddInstruction(value, false);
                                case 0x00000002:
                                    return new LogicalShiftLeftInstruction(value, false);
                                case 0x00000003:
                                    return new SubtractInstruction(value, false);
                                case 0x00000004:
                                    return new LogicalShiftRightInstruction(value, false);
                                case 0x00000005:
                                    return new AndInstruction(value, false);
                                case 0x00000006:
                                    return new ArithmeticShiftRightInstruction(value, false);
                                case 0x00000007:
                                    return new OrInstruction(value, false);
                            }

                            break;
                    }

                    break;
            }

            // It's not an instruction we know about. Ask any extended
            // decoders whether they understand it. Locking is OK since
            // this should happen very rarely, if ever.
            _lock.EnterReadLock();

            try
            {
                foreach (var insn in _fetchers.Select(dec => dec(value, false)).Where(insn => insn != null))
                    return insn;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Doesn't look like a valid 32-bit instruction.
            return null;
        }
    }
}
