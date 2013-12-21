using System;

namespace Lycus.Satori.Instructions
{
    public sealed class StoreDisplacementInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "str"; }
        }

        public StoreDisplacementInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public StoreDisplacementInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
