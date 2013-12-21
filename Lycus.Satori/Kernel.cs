using System;
using System.Diagnostics.CodeAnalysis;

namespace Lycus.Satori
{
    /// <summary>
    /// Represents a kernel.
    ///
    /// Kernels implement functionality such as system calls,
    /// debugging support, and host system access. In other words,
    /// they are used to implement operating systems that run on the
    /// host system and which the Epiphany cores call up to.
    /// </summary>
    public abstract class Kernel : IDisposable
    {
        /// <summary>
        /// Indicates a successful system call.
        /// </summary>
        public const int SystemCallSuccess = 0;

        /// <summary>
        /// Gets the capabilities of this kernel.
        /// </summary>
        public abstract Capabilities Capabilities { get; }

        /// <summary>
        /// Performs a system call.
        /// </summary>
        /// <param name="core">The core performing the call.</param>
        /// <param name="r3">The `r3` register value just before the
        /// `TRAP` instruction. This indicates the system call ID.
        /// This should be set by the system call routine to indicate
        /// whether the call was successful.</param>
        /// <param name="r0">The `r0` register value just before the
        /// `TRAP` instruction. This may be set to some kind of
        /// return value if required for the particular system call.</param>
        /// <param name="r1">The `r1` register value just before the
        /// `TRAP` instruction.</param>
        /// <param name="r2">The `r2` register value just before the
        /// `TRAP` instruction.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "r")]
        public virtual void SystemCall(Core core, ref int r3, ref int r0, int r1, int r2)
        {
            throw new NotSupportedException("This kernel does not implement system calls.");
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~Kernel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
