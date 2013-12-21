using System;

namespace Lycus.Satori.Instructions
{
    public sealed class ArithmeticShiftRightInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "asr"; }
        }

        public ArithmeticShiftRightInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public ArithmeticShiftRightInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
