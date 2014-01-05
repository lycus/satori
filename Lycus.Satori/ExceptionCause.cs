namespace Lycus.Satori
{
    /// <summary>
    /// Indicates the cause of a software exception or user
    /// interrupt.
    /// </summary>
    public enum ExceptionCause
    {
        /// <summary>
        /// No cause.
        /// </summary>
        None,
        /// <summary>
        /// Triggered by an `UNIMPL` instruction.
        /// </summary>
        Unimplemented,
        /// <summary>
        /// Triggered by an `SWI` instruction.
        /// </summary>
        SoftwareInterrupt,
        /// <summary>
        /// Unaligned memory access.
        /// </summary>
        UnalignedAccess,
        IllegalAccess,
        /// <summary>
        /// Floating point exception.
        /// </summary>
        FloatingPoint,
    }
}
