using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MoveImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "mov"; }
        }

        public MoveImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public MoveImmediateInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public int DestinationRegister { get; set; }

        [CLSCompliant(false)]
        public uint Immediate { get; set; }

        public override void Decode()
        {
            DestinationRegister = (int)((Value & ~0xFFFF1FFF) >> 13);
            Immediate = (Value & ~0xFFFFE01F) >> 5;

            if (!Is16Bit)
            {
                DestinationRegister |= (int)((Value & ~0x1FFFFFFF) >> 29 << 3);
                Immediate |= (Value & ~0xF00FFFFF) >> 20 << 8;
            }
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            core.Registers[DestinationRegister] = (int)Immediate;

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, 0x{2:X}".Interpolate(Mnemonic, DestinationRegister, Immediate);
        }
    }
}
