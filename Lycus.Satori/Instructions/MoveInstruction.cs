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
            Condition = (ConditionCode)((Value & ~0xFFFFFF0F) >> 4);

            if (Condition == ConditionCode.BranchAndLink)
                throw InstructionException();

            SourceRegister = (int)((Value & ~0xFFFFE3FF) >> 10);
            DestinationRegister = (int)((Value & ~0xFFFF1FFF) >> 13);

            if (!Is16Bit)
            {
                SourceRegister |= (int)((Value & ~0xE3FFFFFF) >> 26 << 3);
                DestinationRegister |= (int)((Value & ~0x1FFFFFF) >> 29 << 3);
            }
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
