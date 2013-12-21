﻿using System;

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

        public override Operation Execute(Core core)
        {
            return Operation.Next;
        }
    }
}
