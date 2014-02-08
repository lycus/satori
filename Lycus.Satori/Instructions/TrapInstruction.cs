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
            Code = (int)Bits.Extract(Value, 10, 6);
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            if (Code == 7 && core.Machine.Kernel.Capabilities.HasFlag(Capabilities.SystemCalls))
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
            {
                switch (Code)
                {
                    case 3:
                        core.ExitCode = core.Registers[0];
                        break;
                    case 4:
                        core.TestPassed = true;
                        break;
                    case 5:
                        core.TestFailed = true;
                        break;
                }

                core.Registers.CoreStatusStore = Bits.Clear(core.Registers.CoreStatus, 0);
            }

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} {1}".Interpolate(Mnemonic, Code);
        }
    }
}
