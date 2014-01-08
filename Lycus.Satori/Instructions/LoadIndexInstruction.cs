using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LoadIndexInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "ldr"; }
        }

        public LoadIndexInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LoadIndexInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public Size Size { get; set; }

        public bool Subtract { get; set; }

        public int SourceRegister { get; set; }

        public int OperandRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 13, 3) | (int)Bits.Extract(Value, 29, 3) << 3;
            OperandRegister = (int)Bits.Extract(Value, 7, 3) | (int)Bits.Extract(Value, 23, 3) << 3;
            DestinationRegister = (int)Bits.Extract(Value, 10, 3) | (int)Bits.Extract(Value, 26, 3) << 3;
            Size = (Size)Bits.Extract(Value, 5, 2);
            Subtract = Bits.Check(Value, 20);

            if (Size == Size.Int64 && DestinationRegister % 2 != 0)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rn = (uint)core.Registers[SourceRegister];
            var rm = (uint)core.Registers[OperandRegister];
            var addr = Subtract ? rn - rm : rn + rm;

            switch (Size)
            {
                case Size.Int8:
                    core.Registers[DestinationRegister] = core.Machine.Memory.ReadSByte(core, addr);
                    break;
                case Size.Int16:
                    core.Registers[DestinationRegister] = core.Machine.Memory.ReadInt16(core, addr);
                    break;
                case Size.Int32:
                    core.Registers[DestinationRegister] = core.Machine.Memory.ReadInt32(core, addr);
                    break;
                case Size.Int64:
                    var rd = core.Machine.Memory.ReadInt64(core, addr);

                    core.Registers[DestinationRegister] = (int)Bits.Extract(rd, 0, 32);
                    core.Registers[DestinationRegister + 1] = (int)Bits.Extract(rd, 32, 32);
                    break;
            }

            return Operation.Next;
        }
    }
}
