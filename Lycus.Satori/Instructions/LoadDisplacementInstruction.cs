using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LoadDisplacementInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "ldr"; }
        }

        public LoadDisplacementInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LoadDisplacementInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
