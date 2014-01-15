using System;

namespace Lycus.Satori.Instructions
{
    public sealed class TestSetInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "testset"; }
        }

        public TestSetInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public TestSetInstruction(uint value)
            : base(value, false)
        {
        }

        public Size Size { get; set; }

        public int SourceRegister { get; set; }

        public int OperandRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 13, 3) | (int)Bits.Extract(Value, 29, 3) << 3;
            OperandRegister = (int)Bits.Extract(Value, 7, 3) | (int)Bits.Extract(Value, 23, 3) << 3;
            DestinationRegister = (int)Bits.Extract(Value, 10, 3) | (int)Bits.Extract(Value, 26, 3) << 3;
            Size = (Size)Bits.Extract(Value, 5, 2);
            Bits.Check(Value, 20);
        }

        public override void Check()
        {
            if (Size != Size.Int32)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var addr = (uint)(core.Registers[SourceRegister] +
                core.Registers[OperandRegister]);

            // This is a bit of a silly restriction in the architecture. If
            // a core wants to `TESTSET` on a location within its local memory,
            // it has to compute the global address of that location and use
            // that with the `TESTSET`.
            if (CoreId.FromAddress(addr) == CoreId.Current)
                throw new MemoryException(
                    "Attempted local TESTSET at 0x{0:X8}.".Interpolate(addr),
                    addr, false);

            Core tgt;
            byte[] bank;
            object @lock;

            core.Machine.Memory.Translate(core, addr,
                out tgt, out bank, out @lock);

            // Atomic instructions are only supported on the local memory in
            // the Epiphany cores. In other words, no external memory.
            if (tgt == null)
                throw new MemoryException(
                    "Attempted external TESTSET at 0x{0:X8}.".Interpolate(addr),
                    addr, false);

            int value;

            lock (@lock)
            {
                value = core.Machine.Memory.ReadInt32(core, addr);

                if (value != 0)
                    core.Machine.Memory.Write(core, addr, core.Registers[DestinationRegister]);
            }

            core.Registers[DestinationRegister] = value;

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, [r{2}, r{3}]".Interpolate(Mnemonic, DestinationRegister,
                SourceRegister, OperandRegister);
        }
    }
}
