using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LogicalShiftLeftImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "lsl"; }
        }

        public LogicalShiftLeftImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LogicalShiftLeftImmediateInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
