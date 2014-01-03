using System;

namespace Lycus.Satori.Instructions
{
    public sealed class SoftwareInterruptInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "swi"; }
        }

        public SoftwareInterruptInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public SoftwareInterruptInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            core.Interrupts.Trigger(Interrupt.SoftwareException, ExceptionCause.SoftwareInterrupt);

            return Operation.Next;
        }
    }
}
