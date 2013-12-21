using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "float"; }
        }

        public FloatInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
