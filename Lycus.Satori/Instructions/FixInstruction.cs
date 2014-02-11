using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FixInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fix"; }
        }

        public FixInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FixInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public int SourceRegister { get; set; }

        public int OperandRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Decode()
        {
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            OperandRegister = (int)Bits.Extract(Value, 7, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            OperandRegister |= (int)Bits.Extract(Value, 23, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override void Check()
        {
            // The `Rm` field isn't actually used for anything, but must
            // equal the `Rn` field.
            if (OperandRegister != SourceRegister)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var src = core.Registers[SourceRegister].CoerceToSingle();

            int result;

            if (float.IsNaN(src))
                result = src.IsNegative() ? int.MinValue : int.MaxValue;
            else if (src.IsDenormal())
                result = 0;
            else if (Bits.Check(core.Registers.CoreConfig, 0))
                result = (int)Math.Truncate(src);
            else
                result = (int)Math.Round(src);

            core.Registers[DestinationRegister] = result;

            if (float.IsInfinity(src) && Bits.Check(core.Registers.CoreConfig, 1) ||
                src.IsDenormal() && Bits.Check(core.Registers.CoreConfig, 3))
                core.Interrupts.Trigger(Interrupt.SoftwareException, ExceptionCause.FloatingPoint);

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, r{2}".Interpolate(Mnemonic, DestinationRegister, SourceRegister);
        }
    }
}
