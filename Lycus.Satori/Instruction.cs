using System;

namespace Lycus.Satori
{
    public abstract class Instruction
    {
        [CLSCompliant(false)]
        public uint Value { get; private set; }

        public bool Is16Bit { get; private set; }

        public abstract string Mnemonic { get; }

        public virtual bool IsTimed
        {
            get { return false; }
        }

        [CLSCompliant(false)]
        protected Instruction(uint value, bool is16Bit)
        {
            Value = value;
            Is16Bit = is16Bit;
        }

        protected InstructionException InstructionException()
        {
            return new InstructionException(
                "Invalid {0} instruction encountered.".Interpolate(Mnemonic),
                Value, Is16Bit);
        }

        public virtual void Encode()
        {
        }

        public virtual void Decode()
        {
        }

        public abstract Operation Execute(Core core);

        public override string ToString()
        {
            return Mnemonic;
        }
    }
}
