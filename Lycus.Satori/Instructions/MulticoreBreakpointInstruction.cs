using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MulticoreBreakpointInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "mbkpt"; }
        }

        public MulticoreBreakpointInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public MulticoreBreakpointInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            // Currently, we simply ignore this.
            return Operation.Next;
        }
    }
}
