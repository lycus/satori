using System;

namespace Lycus.Satori.Instructions
{
    public sealed class BreakpointInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "bkpt"; }
        }

        public BreakpointInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public BreakpointInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            // Currently, we simply ignore this.
            return Operation.Next;
        }
    }
}
