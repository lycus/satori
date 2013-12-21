using System;

namespace Lycus.Satori.Instructions
{
    public sealed class JumpInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "jr"; }
        }

        public JumpInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public JumpInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public int SourceRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)((Value & ~0xFFFFE3FF) >> 10);

            if (!Is16Bit)
                SourceRegister |= (int)((Value & ~0xE3FFFFFF) >> 26 << 3);
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            core.Registers.ProgramCounter = (uint)core.Registers[SourceRegister];

            return Operation.None;
        }

        public override string ToString()
        {
            return "{0} r{1}".Interpolate(Mnemonic, SourceRegister);
        }
    }
}
