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

            // Set `HALT` and `MBKPT_FLAG` on all cores.
            foreach (var c in core.Machine.Cores)
            {
                var ds = c.Registers.DebugStatus;

                ds = Bits.Set(ds, 0);
                ds = Bits.Set(ds, 2);

                c.Registers.DebugStatus = ds;
            }

            return Operation.Next;
        }
    }
}
