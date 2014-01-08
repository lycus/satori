using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LoadDisplacementInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "ldr"; }
        }

        public LoadDisplacementInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LoadDisplacementInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public Size Size { get; set; }

        [CLSCompliant(false)]
        public uint Immediate { get; set; }

        public bool Subtract { get; set; }

        public int SourceRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);
            Immediate = Bits.Extract(Value, 7, 3);
            Size = (Size)Bits.Extract(Value, 5, 2);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
            Immediate |= Bits.Extract(Value, 16, 8) << 8;
            Subtract = Bits.Check(Value, 24);

            if (Size == Size.Int64 && DestinationRegister % 2 != 0)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rn = (uint)core.Registers[SourceRegister];
            var imm = Immediate << (int)Size;
            var addr = Subtract ? rn - imm : rn + imm;

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
