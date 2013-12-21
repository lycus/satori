namespace Lycus.Satori
{
    /// <summary>
    /// Represents a condition code associated with an instruction.
    /// Instructions with condition codes are only executed if the
    /// specified condition is true.
    /// </summary>
    public enum ConditionCode
    {
        Equal = 0x0,
        NotEqual = 0x1,
        UnsignedGreaterThan = 0x2,
        UnsignedGreaterThanOrEqual = 0x3,
        UnsignedLessThanOrEqual = 0x4,
        UnsignedLessThan = 0x5,
        SignedGreaterThan = 0x6,
        SignedGreaterThanOrEqual = 0x7,
        SignedLessThan = 0x8,
        SignedLessThanOrEqual = 0x9,
        FloatEqual = 0xA,
        FloatNotEqual = 0xB,
        FloatLessThan = 0xC,
        FloatLessThanOrEqual = 0xD,
        /// <summary>
        /// Indicates an unconditional operation.
        /// </summary>
        Unconditional = 0xE,
        /// <summary>
        /// Indicates an unconditional branch, but also that the
        /// `LR` register should be set to the address of the
        /// instruction following the one using this condition
        /// code. This is only valid for the `B` instruction.
        /// </summary>
        BranchAndLink = 0xF,
    }
}
