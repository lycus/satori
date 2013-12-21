using System;

namespace Lycus.Satori.Instructions
{
    public sealed class ArithmeticShiftRightImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "asr"; }
        }

        public ArithmeticShiftRightImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public ArithmeticShiftRightImmediateInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
