namespace Lycus.Satori
{
    /// <summary>
    /// Indicates the action the simulator should take when it has
    /// finished executing an instruction.
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// Continue fetching instructions, but don't increment `PC`.
        /// </summary>
        None,
        /// <summary>
        /// Increment `PC` and continue fetching instructions.
        /// </summary>
        Next,
        /// <summary>
        /// Enter the idle state. In this state, a core will not
        /// perform any work until an interrupt wakes it up.
        ///
        /// This operation is mainly intended for implementing the
        /// `IDLE` instruction.
        /// </summary>
        Idle,
    }
}
