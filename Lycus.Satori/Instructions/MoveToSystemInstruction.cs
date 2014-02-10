using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MoveToSystemInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "movts"; }
        }

        public MoveToSystemInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public MoveToSystemInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public RegisterGroup RegisterGroup { get; set; }

        public int SourceRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);

            if (Is16Bit)
                return;

            RegisterGroup = (RegisterGroup)Bits.Extract(Value, 20, 2);
            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var addr = Memory.RegisterFileAddress + 0x400 +
                (uint)RegisterGroup * 0x100 +
                (uint)SourceRegister * sizeof(uint);

            core.Machine.Memory.Write(core, addr, core.Registers[SourceRegister]);

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} {1}, r{2}".Interpolate(
                Mnemonic,
                MoveFromSystemInstruction.StringizeRegister(RegisterGroup, DestinationRegister),
                SourceRegister);
        }
    }
}
