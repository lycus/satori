using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LogicalShiftRightInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "lsr"; }
        }

        public LogicalShiftRightInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LogicalShiftRightInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
