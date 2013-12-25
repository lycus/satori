using System.Diagnostics.CodeAnalysis;

namespace Lycus.Satori
{
    /// <summary>
    /// Represents a version of the Epiphany architecture.
    /// </summary>
    public enum Architecture
    {
        /// <summary>
        /// The third generation of the Epiphany architecture.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "III")]
        // ReSharper disable once InconsistentNaming
        EpiphanyIII,
        /// <summary>
        /// The fourth generation of the Epiphany architecture.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        EpiphanyIV,
    }
}
