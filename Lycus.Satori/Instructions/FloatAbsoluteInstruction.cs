using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatAbsoluteInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fabs"; }
        }

        public FloatAbsoluteInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatAbsoluteInstruction(uint value, bool is16Bit)
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

        public override void Check()
        {
            // The `Rm` field isn't actually used for anything, but must
            // equal the `Rn` field.
            if (OperandRegister != SourceRegister)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rn = core.Registers[SourceRegister];
            var rd = rn.CoerceToSingle();

            if (rd.IsDenormal())
                rd = rd.ToZero();

            rd = rd.ToPositive();

            core.Registers[DestinationRegister] = rd.CoerceToInt32();

            core.UpdateFlagsB(
                rd == 0.0f,
                rd.IsNegative(),
                rd.ExtractUnbiasedExponent() > 127,
                float.IsNaN(rn),
                rd.ExtractUnbiasedExponent() < -126);

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, r{2}".Interpolate(Mnemonic, DestinationRegister, SourceRegister);
        }
    }
}
