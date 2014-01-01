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

        public int Size { get; set; }

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
            Size = (int)Bits.Extract(Value, 5, 2);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
            Immediate |= Bits.Extract(Value, 16, 8) << 8;
            Subtract = Bits.Check(Value, 24);

            if (Size == 0x3 && SourceRegister % 2 != 0)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            return Operation.Next;
        }
    }
}
