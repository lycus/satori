using System;

namespace Lycus.Satori.Instructions
{
    public sealed class TestSetInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "testset"; }
        }

        public TestSetInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public TestSetInstruction(uint value)
            : base(value, false)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
