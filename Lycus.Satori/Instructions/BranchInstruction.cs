using System;

namespace Lycus.Satori.Instructions
{
    public sealed class BranchInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "b"; }
        }

        public BranchInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public BranchInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public ConditionCode Condition { get; set; }

        public int Immediate { get; set; }

        public override void Decode()
        {
            Condition = (ConditionCode)Bits.Extract(Value, 4, 4);
            Immediate = (int)Bits.Extract(Value, 8, Is16Bit ? 8 : 24);
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            if (!core.EvaluateCondition(Condition))
                return Operation.Next;

            if (Condition == ConditionCode.BranchAndLink)
                core.Registers[14] = (int)(core.Registers.ProgramCounter + (Is16Bit ? sizeof(ushort) : sizeof(uint)));

            // The immediate is actually the amount of 16-bit instructions
            // to jump back/ahead. So multiply it by 2 to get the real offset
            // to add to the PC.
            core.Registers.ProgramCounter += (uint)(Immediate * sizeof(ushort));

            return Operation.None;
        }

        public override string ToString()
        {
            return "{0}{1} 0x{2:X}".Interpolate(Mnemonic, Condition.ToAssemblyString(),
                Immediate * sizeof(ushort));
        }
    }
}
