using System;

namespace Lycus.Satori.Instructions
{
    public sealed class SynchronizeInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "sync"; }
        }

        public SynchronizeInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public SynchronizeInstruction(uint value)
            : base(value, true)
        {
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            // Trigger a synchronize interrupt on all cores, thereby waking
            // them up if they're in an idle state (the `IDLE` instruction).
            foreach (var c in core.Machine.Cores)
                c.Interrupts.Trigger(Interrupt.SynchronizeSignal, ExceptionCause.None);

            return Operation.Next;
        }
    }
}
