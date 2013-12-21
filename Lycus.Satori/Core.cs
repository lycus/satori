using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Lycus.Satori
{
    /// <summary>
    /// Represents an Epiphany eCore.
    /// </summary>
    public sealed class Core
    {
        public Machine Machine { get; private set; }

        public CoreId Id { get; private set; }

        public RegisterFile Registers { get; private set; }

        public InterruptController Interrupts { get; private set; }

        public EventTimer Timer { get; private set; }

        public DirectAccessEngine DirectAccess { get; private set; }

        public DebugUnit Debugger { get; private set; }

        internal byte[] Memory { get; private set; }

        internal object MemoryLock { get; private set; }

        internal Task MainTask { get; private set; }

        bool _idle;

        /// <summary>
        /// Fires if the core fails to match a bit pattern to a
        /// known instruction. The core will immediately enter a
        /// halted state after firing this event.
        ///
        /// This event is triggered on the thread pool, so a slow
        /// event handler will not directly affect the core.
        /// </summary>
        public event EventHandler<InvalidInstructionEventArgs> InvalidInstruction;

        /// <summary>
        /// Fires if the core fails to decode an instruction after
        /// its bit pattern has been matched. The core will
        /// immediately enter a halted state after firing this
        /// event.
        ///
        /// This event is triggered on the thread pool, so a slow
        /// event handler will not directly affect the core.
        /// </summary>
        public event EventHandler<InvalidInstructionEncodingEventArgs> InvalidInstructionEncoding;

        /// <summary>
        /// Fires if the core attempts to access an invalid memory
        /// location, whether through load/store instructions or
        /// DMA. The core will immediately enter a halted state
        /// after firing this event.
        ///
        /// This event is triggered on the thread pool, so a slow
        /// event handler will not directly affect the core.
        /// </summary>
        public event EventHandler<InvalidMemoryAccessEventArgs> InvalidMemoryAccess;

        internal Core(Machine machine, CoreId id)
        {
            Machine = machine;
            Id = id;
            Registers = new RegisterFile(this);
            Interrupts = new InterruptController(this);
            Timer = new EventTimer(this);
            DirectAccess = new DirectAccessEngine(this);
            Debugger = new DebugUnit(this);
            Memory = new byte[Lycus.Satori.Memory.LocalMemorySize];
            MemoryLock = new object();
            MainTask = Task.Run(async () => await CoreLoop());
        }

        async Task CoreLoop()
        {
            while (!Machine.Halting)
            {
                Timer.IncrementClockCycles();

                // Perform any DMA work that might have been scheduled.
                // We could, in theory, do this in a separate task, but
                // it would just complicate things for little benefit.
                // Note that this might raise an interrupt (or two).
                DirectAccess.Update();

                var interrupted = Interrupts.Update();

                // If we've been placed into an idle state by the `IDLE`
                // instruction, we need to wait for an interrupt to be
                // delivered before doing more work.
                if (_idle)
                {
                    Timer.IncrementIdleClockCycles();

                    if (!interrupted)
                    {
                        await Task.Delay(Machine.IdleDuration).ConfigureAwait(false);

                        continue;
                    }

                    _idle = false;
                }

                // Has the debug unit requested that we halt? If so, set
                // `ACTIVE` to zero and wait for something external to
                // wake us up.
                if (Debugger.Update())
                {
                    Registers.CoreStatus &= ~(1u << 0);

                    continue;
                }

                // If the `ACTIVE` bit is zero, just sleep until something
                // wakes us up (such as a library user).
                if ((Registers.CoreStatus & 1 << 0) == 0)
                {
                    await Task.Delay(Machine.SleepDuration).ConfigureAwait(false);

                    continue;
                }

                Instruction insn;

                try
                {
                    var pc = Registers.ProgramCounter;

                    Machine.Logger.TraceCore(this, "PC = 0x{0:X8}", pc);

                    // Try to fetch an instruction. First we read a 16-bit
                    // value to check if it's a short instruction. If it is,
                    // then we decode it and process it. Otherwise, we fetch
                    // the whole 32-bit instruction and process that.
                    var shortIns = Machine.Memory.ReadUInt16(this, pc);

                    insn = Machine.Fetcher.Fetch16Bit(shortIns);

                    // Is it a 32-bit instruction, then?
                    if (insn == null)
                    {
                        var fullIns = Machine.Memory.ReadUInt32(this, pc);

                        insn = Machine.Fetcher.Fetch32Bit(fullIns);

                        if (insn == null)
                        {
                            // This instruction is invalid. This is undefined
                            // behavior, so just halt.
                            Registers.CoreStatus &= ~(1u << 0);

                            new Task(() =>
                            {
                                var evt = InvalidInstruction;

                                if (evt != null)
                                    evt(this, new InvalidInstructionEventArgs(pc, fullIns));
                            }).Start();

                            continue;
                        }
                    }
                }
                catch (MemoryException ex)
                {
                    // Bad PC. This is undefined behavior, so just halt.
                    Registers.CoreStatus &= ~(1u << 0);

                    new Task(() =>
                    {
                        var evt = InvalidMemoryAccess;

                        if (evt != null)
                            evt(this, new InvalidMemoryAccessEventArgs(ex.Address, ex.Write));
                    }).Start();

                    continue;
                }

                try
                {
                    insn.Decode();
                }
                catch (InstructionException)
                {
                    // See comment above.
                    Registers.CoreStatus &= ~(1u << 0);

                    new Task(() =>
                    {
                        var evt = InvalidInstructionEncoding;

                        if (evt != null)
                            evt(this, new InvalidInstructionEncodingEventArgs(insn));
                    }).Start();

                    continue;
                }

                Machine.Logger.TraceCore(this, insn.ToString());

                Operation op;

                try
                {
                    op = insn.Execute(this);
                }
                catch (MemoryException ex)
                {
                    // Bad memory access. This is undefined behavior, so
                    // just halt.
                    Registers.CoreStatus &= ~(1u << 0);

                    new Task(() =>
                    {
                        var evt = InvalidMemoryAccess;

                        if (evt != null)
                            evt(this, new InvalidMemoryAccessEventArgs(ex.Address, ex.Write));
                    }).Start();

                    continue;
                }
                finally
                {
                    if (insn.IsTimed)
                        Timer.IncrementInstructions((Registers.CoreConfig & ~0xFFF1FFFF) >> 17 == 0x4);
                }

                switch (op)
                {
                    case Operation.Next:
                        Registers.ProgramCounter += (uint)(insn.Is16Bit ? sizeof(ushort) : sizeof(uint));
                        break;
                    case Operation.Idle:
                        _idle = true;
                        break;
                }
            }
        }

        public bool EvaluateCondition(ConditionCode condition)
        {
            switch (condition)
            {
                case ConditionCode.Equal:
                    // AZ
                    return (Registers.CoreStatus & 1 << 4) == 1;
                case ConditionCode.NotEqual:
                    // ~AZ
                    return (Registers.CoreStatus & 1 << 4) == 0;
                case ConditionCode.UnsignedGreaterThan:
                    // ~AZ & AC
                    return (Registers.CoreStatus & 1 << 4) == 0 &&
                        (Registers.CoreStatus & 1 << 6) == 1;
                case ConditionCode.UnsignedGreaterThanOrEqual:
                    // AC
                    return (Registers.CoreStatus & 1 << 6) == 1;
                case ConditionCode.UnsignedLessThanOrEqual:
                    // AZ | ~AC
                    return (Registers.CoreStatus & 1 << 4) == 1 ||
                        (Registers.CoreStatus & 1 << 6) == 0;
                case ConditionCode.UnsignedLessThan:
                    // ~AC
                    return (Registers.CoreStatus & 1 << 6) == 0;
                case ConditionCode.SignedGreaterThan:
                    // ~AZ & AV == AN
                    return (Registers.CoreStatus & 1 << 4) == 1 &&
                        (Registers.CoreStatus & 1 << 7) ==
                        (Registers.CoreStatus & 1 << 5);
                case ConditionCode.SignedGreaterThanOrEqual:
                    // AV == AN
                    return (Registers.CoreStatus & 1 << 7) ==
                        (Registers.CoreStatus & 1 << 5);
                case ConditionCode.SignedLessThan:
                    // AV != AN
                    return (Registers.CoreStatus & 1 << 7) !=
                        (Registers.CoreStatus & 1 << 5);
                case ConditionCode.SignedLessThanOrEqual:
                    // AZ | AV != AN
                    return (Registers.CoreStatus & 1 << 4) == 1 ||
                        (Registers.CoreStatus & 1 << 7) !=
                        (Registers.CoreStatus & 1 << 5);
                case ConditionCode.FloatEqual:
                    // BZ
                    return (Registers.CoreStatus & 1 << 8) == 1;
                case ConditionCode.FloatNotEqual:
                    // ~BZ
                    return (Registers.CoreStatus & 1 << 8) == 0;
                case ConditionCode.FloatLessThan:
                    // BN & ~BZ
                    return (Registers.CoreStatus & 1 << 9) == 1 &&
                        (Registers.CoreStatus & 1 << 8) == 0;
                case ConditionCode.FloatLessThanOrEqual:
                    // BN | BZ
                    return (Registers.CoreStatus & 1 << 9) == 1 ||
                        (Registers.CoreStatus & 1 << 8) == 1;
                case ConditionCode.Unconditional:
                case ConditionCode.BranchAndLink:
                    return true;
                default:
                    throw new ArgumentException(
                        "Invalid ConditionCode value ({0}).".Interpolate(condition),
                        "condition");
            }
        }
    }
}
