using System;

namespace Lycus.Satori.Instructions
{
    public sealed class MoveInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "mov"; }
        }

        public MoveInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public MoveInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public ConditionCode Condition { get; set; }

        public int SourceRegister { get; set; }

        public int DestinationRegister { get; set; }

        public override void Encode()
        {
            // What a wonderful hack this thing is...
            if (Condition == ConditionCode.BranchAndLink)
                throw InstructionException();
        }

        public override void Decode()
        {
            Condition = (ConditionCode)Bits.Extract(Value, 4, 4);
            SourceRegister = (int)Bits.Extract(Value, 10, 3);
            DestinationRegister = (int)Bits.Extract(Value, 13, 3);

            if (Is16Bit)
                return;

            SourceRegister |= (int)Bits.Extract(Value, 26, 3) << 3;
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;

            if (Condition == ConditionCode.BranchAndLink)
                throw InstructionException();
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            core.Registers[DestinationRegister] = core.Registers[SourceRegister];

            return Operation.Next;
        }

        public override string ToString()
        {
            return "mov{0} r{1}, r{2}".Interpolate(Condition.ToAssemblyString(),
                DestinationRegister, SourceRegister);
        }
    }
}
