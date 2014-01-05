using System;

namespace Lycus.Satori.Instructions
{
    public sealed class ArithmeticShiftRightImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "asr"; }
        }

        public ArithmeticShiftRightImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public ArithmeticShiftRightImmediateInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public int SourceRegister { get; set; }

        public int DestinationRegister { get; set; }

        public int Immediate { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);
            Immediate = (int)Bits.Extract(Value, 5, 5);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister = (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rd = core.Registers[SourceRegister] >>
                Immediate;

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
