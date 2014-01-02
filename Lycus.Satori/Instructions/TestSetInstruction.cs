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

        public bool Subtract { get; set; }

        public int SourceRegister { get; set; }

        public int OperandRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 13, 3) | (int)Bits.Extract(Value, 29, 3) << 3;
            OperandRegister = (int)Bits.Extract(Value, 7, 3) | (int)Bits.Extract(Value, 23, 3) << 3;
            DestinationRegister = (int)Bits.Extract(Value, 10, 3) | (int)Bits.Extract(Value, 26, 3) << 3;
            Subtract = Bits.Check(Value, 20);
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var src1 = (uint)core.Registers[SourceRegister];
            var src2 = (uint)core.Registers[OperandRegister];
            var addr = Subtract ? src1 - src2 : src1 + src2;

            Core tgt;
            byte[] bank;
            object @lock;

            core.Machine.Memory.Translate(core, addr,
                out tgt, out bank, out @lock);

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
    }
}
