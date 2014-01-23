using System;
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

        public bool TestPassed
        {
            get { return _testPassed; }
            internal set { _testPassed = value; }
        }

        public bool TestFailed
        {
            get { return _testFailed; }
            internal set { _testFailed = value; }
        }

        public int ExitCode
        {
            get { return _exitCode; }
            internal set { _exitCode = value; }
        }

        internal byte[] Memory { get; private set; }

        /// <summary>
        /// The local memory lock.
        ///
        /// This is used when memory accesses to this core's
        /// memory happen. Callers can lock on this object to
        /// perform atomic operations on core memory.
        /// </summary>
        public object Lock { get; private set; }

        internal Task MainTask { get; private set; }

        bool _idle;

        volatile bool _testPassed;

        volatile bool _testFailed;

        volatile int _exitCode;

        /// <summary>
        /// Fires on every core tick, regardless of whether any
        /// instruction has been decoded and executed.
        ///
        /// This event is only fired in the simulator.
        ///
        /// This event is triggered on the thread pool, so a slow
        /// event handler will not directly affect the core.
        /// </summary>
        public event EventHandler<TickEventArgs> Tick;

        /// <summary>
        /// Fires if the core encounters an invalid value in the
        /// `PC` register when trying to fetch an instruction.
        ///
        /// This event is only fired in the simulator.
        ///
        /// This event is triggered on the thread pool, so a slow
        /// event handler will not directly affect the core.
        /// </summary>
        public event EventHandler<InvalidProgramCounterEventArgs> InvalidProgramCounter;

        /// <summary>
        /// Fires if the core fails to match a bit pattern to a
        /// known instruction. The core will immediately enter a
        /// halted state after firing this event.
        ///
        /// This event is only fired in the simulator.
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
        /// This event is only fired in the simulator.
        ///
        /// This event is triggered on the thread pool, so a slow
        /// event handler will not directly affect the core.
        /// </summary>
        public event EventHandler<InvalidInstructionEncodingEventArgs> InvalidInstructionEncoding;

        /// <summary>
        /// Fires if a core executes an instruction without any
        /// issues (such as decoding problems, invalid memory
        /// access, etc).
        ///
        /// This event is only fired in the simulator.
        ///
        /// This event is triggered on the thread pool, so a slow
        /// event handler will not directly affect the core.
        /// </summary>
        public event EventHandler<ValidInstructionEventArgs> ValidInstruction;

        /// <summary>
        /// Fires if the core attempts to access an invalid memory
        /// location, whether through load/store instructions or
        /// DMA. The core will immediately enter a halted state
        /// after firing this event.
        ///
        /// This event is only fired in the simulator.
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
            Memory = new byte[Satori.Memory.LocalMemorySize];
            Lock = new object();
            MainTask = Task.Run(async () => await CoreLoop());
        }

        async Task CoreLoop()
        {
            while (!Machine.Halting)
            {
                if (Id != new CoreId(1, 1))
                    await Task.Delay(1000000000);

                var tevt = Tick;

                if (tevt != null)
                    new Task(() => tevt(this, new TickEventArgs(_idle))).Start();

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
                    Registers.CoreStatus = Bits.Clear(Registers.CoreStatus, 0);

                    continue;
                }

                // If the `ACTIVE` bit is zero, just sleep until something
                // wakes us up (such as a library user).
                if (!Bits.Check(Registers.CoreStatus, 0))
                {
                    await Task.Delay(Machine.SleepDuration).ConfigureAwait(false);

                    continue;
                }

                var pc = Registers.ProgramCounter;

                // If the address in `PC` isn't aligned on a 2-byte boundary
                // undefined behavior results, so halt.
                if (pc % sizeof(ushort) != 0)
                {
                    Registers.CoreStatus = Bits.Clear(Registers.CoreStatus, 0);

                    var evt = InvalidProgramCounter;

                    if (evt != null)
                        new Task(() => evt(this, new InvalidProgramCounterEventArgs(pc))).Start();

                    continue;
                }

                Instruction insn;

                try
                {

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
                            Registers.CoreStatus = Bits.Clear(Registers.CoreStatus, 0);

                            var evt = InvalidInstruction;

                            if (evt != null)
                                new Task(() => evt(this, new InvalidInstructionEventArgs(pc, fullIns))).Start();

                            continue;
                        }
                    }
                }
                catch (MemoryException ex)
                {
                    // Bad PC. This is undefined behavior, so just halt.
                    Registers.CoreStatus = Bits.Clear(Registers.CoreStatus, 0);

                    var evt = InvalidMemoryAccess;

                    if (evt != null)
                        new Task(() => evt(this, new InvalidMemoryAccessEventArgs(ex.Address, ex.Write))).Start();

                    continue;
                }

                insn.Decode();

                try
                {
                    insn.Check();
                }
                catch (InstructionException)
                {
                    // See comment above.
                    Registers.CoreStatus = Bits.Clear(Registers.CoreStatus, 0);

                    var evt = InvalidInstructionEncoding;

                    if (evt != null)
                        new Task(() => evt(this, new InvalidInstructionEncodingEventArgs(insn))).Start();

                    continue;
                }

                Machine.Logger.TraceCore(this, "0x{0:X8}: {1}", insn.Value, insn);

                var isInt = Bits.Extract(Registers.CoreConfig, 17, 3) == 0x4;

                Operation op;

                try
                {
                    op = insn.Execute(this);
                }
                catch (MemoryException ex)
                {
                    // Bad memory access. This is undefined behavior, so
                    // just halt.
                    Registers.CoreStatus = Bits.Clear(Registers.CoreStatus, 0);

                    var evt = InvalidMemoryAccess;

                    if (evt != null)
                        new Task(() => evt(this, new InvalidMemoryAccessEventArgs(ex.Address, ex.Write))).Start();

                    continue;
                }
                catch (DataMisalignedException)
                {
                    // We're not required to finish any memory transaction
                    // if a memory access is unaligned, so just deliver the
                    // interrupt and move on.
                    Interrupts.Trigger(Interrupt.SoftwareException,
                        ExceptionCause.UnalignedAccess);

                    continue;
                }

                if (insn.IsTimed)
                    Timer.IncrementInstructions(isInt);

                var vevt = ValidInstruction;

                if (vevt != null)
                    new Task(() => vevt(this, new ValidInstructionEventArgs(insn))).Start();

                if (op == Operation.Idle)
                    _idle = true;

                var cpc = Registers.ProgramCounter;
                var lc = Registers.LoopCounter;

                // If we've reached the end of a hardware loop, jump to
                // the starting point.
                if (cpc == Registers.LoopEnd && lc > 0)
                {
                    Registers.ProgramCounter = Registers.LoopStart;
                    Registers.LoopCounter = lc - 1;

                    // TODO: Check for various undefined behavior as
                    // specified in the chapter about hardware loops in
                    // the manual.
                }
                else if (op == Operation.Next)
                    Registers.ProgramCounter = cpc + (uint)(insn.Is16Bit ? sizeof(ushort) : sizeof(uint));

                if (Id == new CoreId(1, 1))
                    Console.ReadLine();
            }
        }

        public bool EvaluateCondition(ConditionCode condition)
        {
            var status = Registers.CoreStatus;

            var az = Bits.Check(status, 4);
            var an = Bits.Check(status, 5);
            var ac = Bits.Check(status, 6);
            var av = Bits.Check(status, 7);
            var bz = Bits.Check(status, 8);
            var bn = Bits.Check(status, 9);

            switch (condition)
            {
                case ConditionCode.Equal:
                    // AZ
                    return az;
                case ConditionCode.NotEqual:
                    // ~AZ
                    return !az;
                case ConditionCode.UnsignedGreaterThan:
                    // ~AZ & AC
                    return !az & ac;
                case ConditionCode.UnsignedGreaterThanOrEqual:
                    // AC
                    return ac;
                case ConditionCode.UnsignedLessThanOrEqual:
                    // AZ | ~AC
                    return az | !ac;
                case ConditionCode.UnsignedLessThan:
                    // ~AC
                    return !ac;
                case ConditionCode.SignedGreaterThan:
                    // ~AZ & AV == AN
                    return !az & av == an;
                case ConditionCode.SignedGreaterThanOrEqual:
                    // AV == AN
                    return av == an;
                case ConditionCode.SignedLessThan:
                    // AV != AN
                    return av != an;
                case ConditionCode.SignedLessThanOrEqual:
                    // AZ | AV != AN
                    return az | av != an;
                case ConditionCode.FloatEqual:
                    // BZ
                    return bz;
                case ConditionCode.FloatNotEqual:
                    // ~BZ
                    return !bz;
                case ConditionCode.FloatLessThan:
                    // BN & ~BZ
                    return bn & !bz;
                case ConditionCode.FloatLessThanOrEqual:
                    // BN | BZ
                    return bn | bz;
                case ConditionCode.Unconditional:
                case ConditionCode.BranchAndLink:
                    return true;
                default:
                    throw new ArgumentException(
                        "Invalid ConditionCode value ({0}).".Interpolate(condition),
                        "condition");
            }
        }

        internal void UpdateFlagsA(bool az, bool an, bool ac, bool av)
        {
            var status = Registers.CoreStatus;

            status = Bits.Insert(status, az ? 1u : 0, 4, 1);
            status = Bits.Insert(status, an ? 1u : 0, 5, 1);
            status = Bits.Insert(status, ac ? 1u : 0, 6, 1);
            status = Bits.Insert(status, av ? 1u : 0, 7, 1);

            // If `AV` is set, set `AVS`, but don't clear it otherwise.
            if (av)
                status = Bits.Insert(status, 1, 12, 1);

            Registers.CoreStatus = status;
        }

        internal void UpdateFlagsB(bool bz, bool bn, bool? bv, bool bi, bool bu)
        {
            var status = Registers.CoreStatus;

            status = Bits.Insert(status, bz ? 1u : 0, 8, 1);
            status = Bits.Insert(status, bn ? 1u : 0, 9, 1);

            if (bv != null)
            {
                status = Bits.Insert(status, (bool)bv ? 1u : 0, 10, 1);

                // If `BV` is set, set `BVS`, but don't clear it otherwise.
                if ((bool)bv)
                    status = Bits.Insert(status, 1, 14, 1);
            }

            // We set `BIS`, but never clear it.
            if (bi)
                status = Bits.Insert(status, 1, 13, 1);

            // We set `BUS`, but never clear it.
            if (bu)
                status = Bits.Insert(status, 1, 15, 1);

            Registers.CoreStatus = status;
        }
    }
}
