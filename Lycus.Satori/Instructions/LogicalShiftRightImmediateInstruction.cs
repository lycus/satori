using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LogicalShiftRightImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "lsr"; }
        }

        public LogicalShiftRightImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LogicalShiftRightImmediateInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
