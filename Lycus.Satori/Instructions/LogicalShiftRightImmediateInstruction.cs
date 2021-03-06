﻿using System;

namespace Lycus.Satori.Instructions
{
    public sealed class LogicalShiftRightImmediateInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "lsr"; }
        }

        public LogicalShiftRightImmediateInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public LogicalShiftRightImmediateInstruction(uint value, bool is16Bit)
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
            DestinationRegister |= (int)Bits.Extract(Value, 29, 3) << 3;
        }

        public override Operation Execute(Core core)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            var rd = (int)((uint)core.Registers[SourceRegister] >>
                Immediate);

            core.Registers[DestinationRegister] = rd;

            core.UpdateFlagsA(
                rd == 0,
                rd < 0,
                false,
                false);

            return Operation.Next;
        }

        public override string ToString()
        {
            return "{0} r{1}, r{2}, 0x{3:X}".Interpolate(Mnemonic,
                DestinationRegister, SourceRegister, Immediate);
        }
    }
}
