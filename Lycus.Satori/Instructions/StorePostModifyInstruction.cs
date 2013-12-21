using System;

namespace Lycus.Satori.Instructions
{
    public sealed class StorePostModifyInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "str"; }
        }

        public StorePostModifyInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public StorePostModifyInstruction(uint value)
            : base(value, false)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
