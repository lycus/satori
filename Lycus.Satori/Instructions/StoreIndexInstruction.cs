using System;

namespace Lycus.Satori.Instructions
{
    public sealed class StoreIndexInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "str"; }
        }

        public StoreIndexInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public StoreIndexInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
