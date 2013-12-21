using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MoveToSystemInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "movts"; }
        }

        public MoveToSystemInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public MoveToSystemInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
