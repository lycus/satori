using System;

namespace Lycus.Satori.Instructions
{
    public sealed class GlobalInterruptEnableInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "gie"; }
        }

        public GlobalInterruptEnableInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public GlobalInterruptEnableInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            core.Registers.CoreStatus &= ~(1u << 1);

            return Operation.Next;
        }
    }
}
