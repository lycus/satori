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

            var lhs = core.Registers[SourceRegister].CoerceToSingle();
            var rhs = core.Registers[OperandRegister].CoerceToSingle();

            float result;

            if (Bits.Extract(core.Registers.CoreConfig, 17, 3) == 0x0)
            {
                if (float.IsNaN(lhs) || float.IsNaN(rhs))
                {
                    // `float.NaN` is a quiet NaN.
                    var nan = float.NaN.CoerceToInt32();
                    var sign = Bits.Check(lhs.CoerceToInt32(), 31) ^
                        Bits.Check(rhs.CoerceToInt32(), 31);

                    result = Bits.Insert(nan, sign ? 1 : 0, 31, 1);
                }
                else
                {
                    if (lhs.IsDenormal())
                        lhs = lhs.ToZero();

                    if (rhs.IsDenormal())
                        rhs = rhs.ToZero();

                    result = lhs * rhs;

                    if (result.IsDenormal())
                        result = result.ToZero();
                }
            }
            else
                result = (lhs.CoerceToInt32() * lhs.CoerceToInt32()).CoerceToSingle();

            core.Registers[DestinationRegister] = result.CoerceToInt32();

            return Operation.Next;
        }
    }
}
