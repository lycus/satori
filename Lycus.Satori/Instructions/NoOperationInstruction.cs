using System;

namespace Lycus.Satori.Instructions
{
    public sealed class NoOperationInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "nop"; }
        }

        public NoOperationInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public NoOperationInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            // Does nothing.
            return Operation.Next;
        }
    }
}
