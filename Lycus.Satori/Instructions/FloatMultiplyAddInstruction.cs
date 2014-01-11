using System;
using System.Linq;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatMultiplyAddInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fmadd"; }
        }

        public FloatMultiplyAddInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatMultiplyAddInstruction(uint value, bool is16Bit)
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

            var src1 = core.Registers[DestinationRegister].CoerceToSingle();
            var src2 = core.Registers[SourceRegister].CoerceToSingle();
            var src3 = core.Registers[OperandRegister].CoerceToSingle();

            float result;

            if (Bits.Extract(core.Registers.CoreConfig, 17, 3) == 0x0)
            {
                if (float.IsNaN(src1) || float.IsNaN(src2) || float.IsNaN(src3))
                {
                    // `float.NaN` is a quiet NaN.
                    var nan = float.NaN.CoerceToInt32();
                    var sign = Bits.Check(src1.CoerceToInt32(), 31) ^
                        Bits.Check(src2.CoerceToInt32(), 31) ^
                        Bits.Check(src3.CoerceToInt32(), 31);

                    result = Bits.Insert(nan, sign ? 1 : 0, 31, 1);
                }
                else
                {
                    if (src1.IsDenormal())
                        src1 = src1.ToZero();

                    if (src2.IsDenormal())
                        src2 = src2.ToZero();

                    if (src3.IsDenormal())
                        src3 = src3.ToZero();

                    result = src1 + src2 * src3;

                    if (result.IsDenormal())
                        result = result.ToZero();
                }
            }
            else
                result = (src1.CoerceToInt32() + src2.CoerceToInt32() * src3.CoerceToInt32()).CoerceToSingle();

            core.Registers[DestinationRegister] = result.CoerceToInt32();

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, r{2}, r{3}".Interpolate(Mnemonic,
                DestinationRegister, SourceRegister, OperandRegister);
        }
    }
}
