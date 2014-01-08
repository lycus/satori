using System;

namespace Lycus.Satori.Instructions
{
    public sealed class StoreDisplacementInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "str"; }
        }

        public StoreDisplacementInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public StoreDisplacementInstruction(uint value, bool is16Bit)
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
            SourceRegister = (int)Bits.Extract(Value, 13, 3);
            DestinationRegister = (int)Bits.Extract(Value, 10, 3);
            Immediate = Bits.Extract(Value, 7, 3);
            Size = (Size)Bits.Extract(Value, 5, 2);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            Immediate |= Bits.Extract(Value, 16, 8) << 8;
            Subtract = Bits.Check(Value, 24);

            if (Size == Size.Int64 && SourceRegister % 2 != 0)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rn = (uint)core.Registers[DestinationRegister];
            var imm = Immediate << (int)Size;
            var addr = Subtract ? rn - imm : rn + imm;

            var rd = core.Registers[SourceRegister];

            switch (Size)
            {
                case Size.Int8:
                    core.Machine.Memory.Write(core, addr, (sbyte)rd);
                    break;
                case Size.Int16:
                    core.Machine.Memory.Write(core, addr, (short)rd);
                    break;
                case Size.Int32:
                    core.Machine.Memory.Write(core, addr, rd);
                    break;
                case Size.Int64:
                    core.Machine.Memory.Write(core, addr,
                        Bits.Insert((long)rd, core.Registers[SourceRegister + 1], 0, 32));
                    break;
            }

            return Operation.Next;
        }
    }
}
