using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LoadPostModifyInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "ldr"; }
        }

        public LoadPostModifyInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public LoadPostModifyInstruction(uint value)
            : base(value, false)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
