﻿using System;

namespace Lycus.Satori.Instructions
{
    public sealed class FloatMultiplyInstruction : Instruction
    {
        public override string Mnemonic
        {
            get { return "fmul"; }
        }

        public FloatMultiplyInstruction(bool is16Bit)
            : this(0, is16Bit)
        {
        }

        [CLSCompliant(false)]
        public FloatMultiplyInstruction(uint value, bool is16Bit)
            : base(value, is16Bit)
        {
        }

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
