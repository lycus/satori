using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LoadPostModifyInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "ldr"; }
        }

        public LoadPostModifyInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public LoadPostModifyInstruction(uint value)
            : base(value, false)
        {
        }

        public int Size { get; set; }

        public bool Subtract { get; set; }

        public int SourceRegister { get; set; }

        public int OperandRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3) | (int)Bits.Extract(Value, 26, 3) << 3;
            OperandRegister = (int)Bits.Extract(Value, 7, 3) | (int)Bits.Extract(Value, 23, 3) << 3;
            DestinationRegister = (int)Bits.Extract(Value, 13, 3) | (int)Bits.Extract(Value, 29, 3) << 3;
            Size = (int)Bits.Extract(Value, 5, 2);
            Subtract = Bits.Check(Value, 20);

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
