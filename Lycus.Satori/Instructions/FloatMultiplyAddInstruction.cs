using System;

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

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
