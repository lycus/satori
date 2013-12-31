using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatMultiplySubtractInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fmsub"; }
        }

        public FloatMultiplySubtractInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatMultiplySubtractInstruction(uint value, bool is16Bit)
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

            var src1 = core.Registers[DestinationRegister];
            var src2 = core.Registers[SourceRegister];
            var src3 = core.Registers[OperandRegister];

            int result;

            if (Bits.Extract(core.Registers.CoreConfig, 17, 3) == 0x4)
                result = src1 - src1 * src3;
            else
                result = (src1.ReinterpretAsSingle() - src2.ReinterpretAsSingle() * src3.ReinterpretAsSingle()).ReinterpretAsInt32();

            core.Registers[DestinationRegister] = result;

            return Operation.Next;
        }
    }
}
