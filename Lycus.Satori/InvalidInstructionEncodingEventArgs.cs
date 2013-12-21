using System;

namespace Lycus.Satori
{
    public class InvalidInstructionEncodingEventArgs : EventArgs
    {
        public Instruction Instruction { get; private set; }

        public InvalidInstructionEncodingEventArgs(Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            Instruction = instruction;
        }
    }
}
