using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FixInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fix"; }
        }

        public FixInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FixInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
