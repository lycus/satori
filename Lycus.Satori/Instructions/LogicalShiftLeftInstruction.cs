using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LogicalShiftLeftInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "lsl"; }
        }

        public LogicalShiftLeftInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LogicalShiftLeftInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
