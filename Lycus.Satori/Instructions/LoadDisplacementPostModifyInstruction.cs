using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LoadDisplacementPostModifyInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "ldr"; }
        }
        
        public LoadDisplacementPostModifyInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LoadDisplacementPostModifyInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
