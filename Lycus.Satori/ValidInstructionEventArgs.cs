using System;

namespace Lycus.Satori
{
    public class ValidInstructionEventArgs : EventArgs
    {
        public Instruction Instruction { get; private set; }

        public ValidInstructionEventArgs(Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            Instruction = instruction;
        }
    }
}
