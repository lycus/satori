using System;

namespace Lycus.Satori.Instructions
{
    public sealed class AddImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "add"; }
        }

        public AddImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public AddImmediateInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
