using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MoveFromSystemInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "movfs"; }
        }

        public MoveFromSystemInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public MoveFromSystemInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public RegisterGroup RegisterGroup { get; set; }

        public int SourceRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);

            if (Is16Bit)
                return;

            RegisterGroup = (RegisterGroup)Bits.Extract(Value, 20, 2);
            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var addr = Memory.RegisterFileAddress + 0x400 +
                (uint)RegisterGroup * 0x100 +
                (uint)SourceRegister * sizeof(uint);

            core.Registers[DestinationRegister] = core.Machine.Memory.ReadInt32(core, addr);

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, r{2}".Interpolate(
                Mnemonic,
                DestinationRegister,
                StringizeRegister(RegisterGroup, SourceRegister));
        }

        internal static string StringizeRegister(RegisterGroup group, int register)
        {
            switch (group)
            {
                case RegisterGroup.CoreControl:
                    switch (register)
                    {
                        case 0:
                            return "config";
                        case 1:
                            return "status";
                        case 2:
                            return "pc";
                        case 3:
                            return "debugstatus";
                        case 5:
                            return "lc";
                        case 6:
                            return "ls";
                        case 7:
                            return "le";
                        case 8:
                            return "iret";
                        case 9:
                            return "imask";
                        case 10:
                            return "ilat";
                        case 11:
                            return "ilatst";
                        case 12:
                            return "ilatcl";
                        case 13:
                            return "ipend";
                        case 14:
                            return "ctimer0";
                        case 15:
                            return "ctimer1";
                        case 16:
                            return "fstatus";
                        case 17:
                            return "debugcmd";
                    }

                    break;
                case RegisterGroup.DirectAccess:
                    switch (register)
                    {
                        case 0:
                            return "dma0config";
                        case 1:
                            return "dma0stride";
                        case 2:
                            return "dma0count";
                        case 3:
                            return "dma0srcaddr";
                        case 4:
                            return "dma0dstaddr";
                        case 5:
                            return "dma0auto0";
                        case 6:
                            return "dma0auto1";
                        case 7:
                            return "dma0status";
                        case 8:
                            return "dma1config";
                        case 9:
                            return "dma1stride";
                        case 10:
                            return "dma1count";
                        case 11:
                            return "dma1srcaddr";
                        case 12:
                            return "dma1dstaddr";
                        case 13:
                            return "dma1auto0";
                        case 14:
                            return "dma1auto1";
                        case 15:
                            return "dma1status";
                    }

                    break;
                case RegisterGroup.MemoryProtection:
                    switch (register)
                    {
                        case 1:
                            return "memstatus";
                        case 2:
                            return "memprotect";
                    }

                    break;
                case RegisterGroup.NodeConfiguration:
                    switch (register)
                    {
                        case 0:
                            return "meshconfig";
                        case 1:
                            return "coreid";
                        case 2:
                            return "multicast";
                        case 3:
                            return "resetcore";
                        case 4:
                            return "cmeshroute";
                        case 5:
                            return "xmeshroute";
                        case 6:
                            return "rmeshroute";
                    }

                    break;
            }

            throw new ArgumentOutOfRangeException(
                "Invalid system register value ({0}).".Interpolate(register),
                "register");
        }
    }
}
