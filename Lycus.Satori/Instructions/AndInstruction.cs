using System;

namespace Lycus.Satori.Instructions
{
    public sealed class AndInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "and"; }
        }

        public AndInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public AndInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public int SourceRegister { get; set; }

        public int OperandRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            OperandRegister = (int)Bits.Extract(Value, 7, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            OperandRegister |= (int)Bits.Extract(Value, 23, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rd = core.Registers[SourceRegister] &
                core.Registers[OperandRegister];

            core.Registers[DestinationRegister] = rd;

            core.UpdateFlagsA(
                rd == 0,
                rd < 0,
                false,
                false);

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, r{2}, r{3}".Interpolate(Mnemonic,
                DestinationRegister, SourceRegister, OperandRegister);
        }
    }
}
