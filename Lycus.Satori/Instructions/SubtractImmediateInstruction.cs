using System;

namespace Lycus.Satori.Instructions
{
    public sealed class SubtractImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "sub"; }
        }

        public SubtractImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public SubtractImmediateInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public int SourceRegister { get; set; }

        public int DestinationRegister { get; set; }

        public int Immediate { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);
            Immediate = (int)Bits.Extract(Value, 7, 3);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister = (int)Bits.Extract(Value, 29, 3) << 3;
            Immediate |= (int)Bits.Extract(Value, 16, 8) << 8;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rn = core.Registers[SourceRegister];
            var rm = Immediate;
            var rd = rn + rm;

            core.Registers[DestinationRegister] = rd;

            core.UpdateFlagsA(
                rd == 0,
                rd < 0,
                rd > rn,
                rd < 0 == rn < 0 && rm < 0 == rd < 0);

            return Operation.Next;
        }
    }
}
