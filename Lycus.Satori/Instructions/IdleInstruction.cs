using System;

namespace Lycus.Satori.Instructions
{
    public sealed class IdleInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "idle"; }
        }

        public IdleInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public IdleInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            // This places the core in an idle state where it doesn't do
            // anything. However, it cannot be woken up by setting `ACTIVE`
            // in `STATUS`; instead, an interrupt (such as interrupt zero,
            // triggered by `SYNC`) must be delivered.
            core.Registers.CoreStatus = Bits.Clear(core.Registers.CoreStatus, 0);

            return Operation.Idle;
        }
    }
}
