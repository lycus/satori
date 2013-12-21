using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatSubtractInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fsub"; }
        }

        public FloatSubtractInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatSubtractInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
