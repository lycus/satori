using System;
using System.ComponentModel;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatAddInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fadd"; }
        }

        public FloatAddInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatAddInstruction(uint value, bool is16Bit)
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

            var rn = core.Registers[SourceRegister].CoerceToSingle();
            var rm = core.Registers[OperandRegister].CoerceToSingle();

            float rd;

            if (Bits.Extract(core.Registers.CoreConfig, 17, 3) == 0x0)
            {
                if (float.IsNaN(rn) || float.IsNaN(rm))
                {
                    // `float.NaN` is a quiet NaN.
                    var nan = float.NaN.CoerceToInt32();
                    var sign = Bits.Check(rn.CoerceToInt32(), 31) ^
                        Bits.Check(rm.CoerceToInt32(), 31);

                    rd = Bits.Insert(nan, sign ? 1 : 0, 31, 1);
                }
                else
                {
                    if (rn.IsDenormal())
                        rn = rn.ToZero();

                    if (rm.IsDenormal())
                        rm = rm.ToZero();

                    rd = rn + rm;

                    if (rd.IsDenormal())
                        rd = rd.ToZero();
                }

                core.UpdateFlagsB(
                    rd == 0.0f,
                    rd.IsNegative(),
                    rd.ExtractUnbiasedExponent() > 127,
                    float.IsNaN(rn) || float.IsNaN(rm),
                    rd.ExtractUnbiasedExponent() < -126);
            }
            else
            {
                var res = rn.CoerceToInt32() + rn.CoerceToInt32();

                rd = res.CoerceToSingle();

                core.UpdateFlagsB(
                    res == 0,
                    res < 0,
                    null,
                    false,
                    false);
            }

            core.Registers[DestinationRegister] = rd.CoerceToInt32();

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, r{2}, r{3}".Interpolate(Mnemonic,
                DestinationRegister, SourceRegister, OperandRegister);
        }
    }
}
