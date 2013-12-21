using System;

namespace Lycus.Satori.Instructions
{
    public sealed class OrInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "orr"; }
        }

        public OrInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public OrInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
