using System;

namespace Lycus.Satori.Instructions
{
    public sealed class TrapInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "trap"; }
        }

        public TrapInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public TrapInstruction(uint value)
            : base(value, true)
        {
        }

        public int Code { get; set; }

        public override void Decode()
        {
            Code = (int)((Value & ~0xFFFF03FF) >> 10);
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            // If we don't support system calls, or the trap code isn't
            // equal to the system call code, simply halt the core.
            if (core.Machine.Kernel.Capabilities.HasFlag(Capabilities.SystemCalls) && Code == 7)
            {
                var r0 = core.Registers[0];
                var r1 = core.Registers[1];
                var r2 = core.Registers[2];
                var r3 = core.Registers[3];

                core.Machine.Kernel.SystemCall(core, ref r3, ref r0, r1, r2);

                core.Registers[0] = r0;
                core.Registers[1] = r1;
                core.Registers[2] = r2;
                core.Registers[3] = r3;
            }
            else
                core.Registers.CoreStatus &= ~(1u << 0);

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} {1}".Interpolate(Mnemonic, Code);
        }
    }
}
