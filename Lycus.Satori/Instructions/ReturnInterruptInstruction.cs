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

            if (core.Interrupts.Current != null)
            {
                core.Registers.InterruptsPending &= ~(1u << (int)core.Interrupts.Current);

                core.Interrupts.Current = null;
            }

            core.Registers.CoreStatus &= ~(1u << 1); // Enable interrupts.
            core.Registers.CoreStatus &= ~(1u << 2); // Back to user mode.
            core.Registers.ProgramCounter = core.Registers.InterruptReturn;

            return Operation.None;
        }
    }
}
