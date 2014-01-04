using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MoveToSystemInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "movts"; }
        }

        public MoveToSystemInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public MoveToSystemInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public int RegisterGroup { get; set; }

        public int SourceRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);

            if (Is16Bit)
                return;

            RegisterGroup = (int)Bits.Extract(Value, 20, 2);
            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var addr = Memory.RegisterFileAddress + 0x400;

            switch (RegisterGroup)
            {
                case 0x0:
                    addr += 0x000;
                    break;
                case 0x1:
                    addr += 0x100;
                    break;
                case 0x2:
                    addr += 0x200;
                    break;
                case 0x3:
                    addr += 0x300;
                    break;
            }

            addr += (uint)DestinationRegister * 4;

            core.Machine.Memory.Write(core, addr, core.Registers[SourceRegister]);

            return Operation.Next;
        }
    }
}
