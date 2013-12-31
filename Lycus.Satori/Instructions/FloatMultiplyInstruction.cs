using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatMultiplyInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fmul"; }
        }

        public FloatMultiplyInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatMultiplyInstruction(uint value, bool is16Bit)
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

            var lhs = core.Registers[SourceRegister];
            var rhs = core.Registers[OperandRegister];

            int result;

            if (Bits.Extract(core.Registers.CoreConfig, 17, 3) == 0x4)
                result = lhs * rhs;
            else
                result = (lhs.CoerceToSingle() * rhs.CoerceToSingle()).CoerceToInt32();

            core.Registers[DestinationRegister] = result;

            return Operation.Next;
        }
    }
}
