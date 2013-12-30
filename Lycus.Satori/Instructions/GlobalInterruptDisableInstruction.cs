using System;

namespace Lycus.Satori.Instructions
{
    public sealed class GlobalInterruptDisableInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "gid"; }
        }

        public GlobalInterruptDisableInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public GlobalInterruptDisableInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            core.Registers.CoreStatus = Bits.Set(core.Registers.CoreStatus, 1);

            return Operation.Next;
        }
    }
}
