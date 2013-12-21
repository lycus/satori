using System;

namespace Lycus.Satori.Instructions
{
    public sealed class UnimplementedInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "unimpl"; }
        }

        public UnimplementedInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public UnimplementedInstruction(uint value)
            : base(value, false)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            core.Interrupts.Trigger(Interrupt.SoftwareException, ExceptionCause.Unimplemented);

            return Operation.Next;
        }
    }
}
