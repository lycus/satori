using System;
using System.Linq;

namespace Lycus.Satori.Instructions
{
    public sealed class WiredAndInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "wand"; }
        }

        public WiredAndInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public WiredAndInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            // Flip the `WAND` bit on.
            core.Registers.CoreStatusStore = Bits.Set(core.Registers.CoreStatus, 3);

            // Check if all cores have the bit set. There is no need for
            // locking here, as the assumption is that cores will only
            // clear the bit in the ISR, which we trigger below.
            var flag = core.Machine.Cores.All(c => Bits.Check(core.Registers.CoreStatus, 3));

            if (!flag)
                return Operation.Next;

            // If the bit is set, trigger a `WAND` interrupt everywhere.
            foreach (var c in core.Machine.Cores)
                c.Interrupts.Trigger(Interrupt.WiredAnd, ExceptionCause.None);

            return Operation.Next;
        }
    }
}
