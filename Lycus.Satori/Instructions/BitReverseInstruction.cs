using System;

namespace Lycus.Satori.Instructions
{
    public sealed class BitReverseInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "bitr"; }
        }

        public BitReverseInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public BitReverseInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
