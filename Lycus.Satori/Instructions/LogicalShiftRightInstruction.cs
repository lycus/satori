using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LogicalShiftRightInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "lsr"; }
        }

        public LogicalShiftRightInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LogicalShiftRightInstruction(uint value, bool is16Bit)
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
            DestinationRegister = (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rd = (int)((uint)core.Registers[SourceRegister] >>
                Bits.Extract(core.Registers[OperandRegister], 0, 5));

            core.Registers[DestinationRegister] = rd;

            core.UpdateFlagsA(
                rd == 0,
                rd < 0,
                false,
                false);

            return Operation.Next;
        }
    }
}
