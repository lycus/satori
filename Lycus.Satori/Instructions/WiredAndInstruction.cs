﻿using System;

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
            core.Registers.CoreStatus |= (1 << 3);

            var flag = true;

            // Check if all cores have the bit set. There is no need for
            // locking here, as the assumption is that cores will only
            // clear the bit in the ISR, which we trigger below.
            foreach (var c in core.Machine.Cores)
            {
                if ((c.Registers.CoreStatus & 1 << 3) == 0)
                {
                    flag = false;

                    break;
                }
            }

            // If the bit is set, trigger a `WAND` interrupt everywhere.
            if (flag)
                foreach (var c in core.Machine.Cores)
                    c.Interrupts.Trigger(Interrupt.WiredAnd, ExceptionCause.None);

            return Operation.Next;
        }
    }
}
