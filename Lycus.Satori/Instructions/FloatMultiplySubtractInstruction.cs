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

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
