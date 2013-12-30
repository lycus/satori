using System;

namespace Lycus.Satori.Instructions
{
    public sealed class ReturnInterruptInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "rti"; }
        }

        public ReturnInterruptInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public ReturnInterruptInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var cur = core.Interrupts.Current;

            if (cur != null)
            {
                core.Registers.InterruptsPending = Bits.Clear(core.Registers.InterruptsPending, (int)cur);

                core.Interrupts.Current = null;
            }

            var status = core.Registers.CoreStatus;

            status = Bits.Clear(status, 1); // Enable interrupts.
            status = Bits.Clear(status, 2); // Back to user mode.

            core.Registers.CoreStatus = status;
            core.Registers.ProgramCounter = core.Registers.InterruptReturn;

            return Operation.None;
        }
    }
}
