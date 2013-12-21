using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MoveHighImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "movt"; }
        }

        public MoveHighImmediateInstruction()
            : this(0)
        {
        }

        [CLSCompliant(false)]
        public MoveHighImmediateInstruction(uint value)
            : base(value, false)
        {
        }

        public int DestinationRegister { get; set; }

        [CLSCompliant(false)]
        public uint Immediate { get; set; }

        public override void Decode()
        {
            DestinationRegister = (int)((Value & ~0xFFFF1FFF) >> 13 | (Value & ~0x1FFFFFFF) >> 29 << 3);
            Immediate = (Value & ~0xFFFFE01F) >> 5 | (Value & ~0xF00FFFFF) >> 20 << 8;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            // The cast to `ushort` discards the higher bits.
            core.Registers[DestinationRegister] =
                (int)((ushort)core.Registers[DestinationRegister] | (Immediate << 16));

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, 0x{2:X}".Interpolate(Mnemonic, DestinationRegister, Immediate);
        }
    }
}
