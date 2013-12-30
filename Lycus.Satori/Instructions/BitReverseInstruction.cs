using System;

namespace Lycus.Satori.Instructions
{
    public sealed class BitReverseInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "bitr"; }
        }

        public BitReverseInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public BitReverseInstruction(uint value, bool is16Bit)
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
            Immediate = (int)Bits.Extract(Value, 5, 5);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister = (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var src = core.Registers[SourceRegister];
            var dst = 0;

            const int bits = sizeof(int) * 8;

            for (var i = 0; i < bits; i++)
            {
                var v = Bits.Extract(src, i, 1);

                dst = Bits.Insert(dst, v, bits - 1 - i, 1);
            }

            core.Registers[DestinationRegister] = dst;

            return Operation.Next;
        }
    }
}
