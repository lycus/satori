using System;

namespace Lycus.Satori.Instructions
{
    public sealed class AndInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "and"; }
        }

        public AndInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public AndInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
