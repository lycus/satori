using System;

namespace Lycus.Satori
{
    public sealed class InterruptController
    {
        public Core Core { get; private set; }

        public Interrupt? Current { get; internal set; }

        readonly object _lock = new object();

        internal InterruptController(Core core)
        {
            Core = core;
        }

        internal bool Update()
        {
            var status = Core.Registers.CoreStatus;

            // Are interrupts disabled entirely?
            if (Bits.Check(status, 1))
                return false;

            var ilat = Core.Registers.InterruptLatch;
            var ipend = Core.Registers.InterruptsPending;

            lock (_lock)
            {
                for (var i = 0; i < 9; i++)
                {
                    // Is the latch bit set?
                    if (!Bits.Check(ilat, i))
                        continue;

                    // Are we masked?
                    if (Bits.Check(Core.Registers.InterruptMask, i))
                        continue;

                    // Are any higher-priority interrupts already being
                    // handled? For example, if we get a timer interrupt
                    // while a `SYNC` interrupt is being handled, we need
                    // to ignore the timer interrupt as it's lower priority.
                    if (Bits.Extract(ipend, 0, i - 1) != 0)
                        continue;

                    Current = (Interrupt)i;

                    // Okay, we're good to go. Now:
                    //
                    // 1. Save the `PC` in `IRET`.
                    // 2. Unset the latch bit.
                    // 3. Set the pending bit.
                    // 4. Disable all interrupts.
                    // 5. Enter kernel mode if needed.
                    // 6. Branch to the IVT entry.

                    Core.Registers.InterruptReturn = Core.Registers.ProgramCounter;
                    Core.Registers.InterruptLatch = Bits.Clear(ilat, i);
                    Core.Registers.InterruptsPending = Bits.Set(ipend, i);

                    status = Bits.Set(status, 1);

                    if (Bits.Check(Core.Registers.CoreConfig, 25))
                        status = Bits.Set(status, 2);

                    Core.Registers.CoreStatusStore = status;
                    Core.Registers.ProgramCounter = (uint)(Memory.InterruptVectorAddress + i * sizeof(uint));

                    return true;
                }
            }

            return false;
        }

        public void Trigger(Interrupt priority, ExceptionCause cause)
        {
            switch (priority)
            {
                case Interrupt.SynchronizeSignal:
                case Interrupt.MemoryFault:
                case Interrupt.Timer0:
                case Interrupt.Timer1:
                case Interrupt.Message:
                case Interrupt.DirectAccess0:
                case Interrupt.DirectAccess1:
                case Interrupt.WiredAnd:
                    if (cause != ExceptionCause.None)
                        throw new ArgumentException(
                            "Exception cause not allowed for {0}.".Interpolate(priority),
                            "priority");

                    break;
                case Interrupt.SoftwareException:
                case Interrupt.User:
                    if (cause == ExceptionCause.None)
                        throw new ArgumentException(
                            "Exception cause required for {0}.".Interpolate(priority),
                            "priority");

                    break;
                default:
                    throw new ArgumentException(
                        "Invalid Interrupt value ({0}).".Interpolate(priority),
                        "priority");
            }

            var exCause = 0;
            var iv = Core.Machine.Architecture == Architecture.EpiphanyIV;

            switch (cause)
            {
                case ExceptionCause.None:
                    break;
                case ExceptionCause.Unimplemented:
                    exCause = iv ? 0xF : 0x4;
                    break;
                case ExceptionCause.SoftwareInterrupt:
                    exCause = iv ? 0xE : 0x1;
                    break;
                case ExceptionCause.UnalignedAccess:
                    exCause = iv ? 0xD : 0x2;
                    break;
                case ExceptionCause.IllegalAccess:
                    exCause = iv ? 0xC : 0x5;
                    break;
                case ExceptionCause.FloatingPoint:
                    exCause = iv ? 0x7 : 0x3;
                    break;
                default:
                    throw new ArgumentException(
                        "Invalid ExceptionCause value ({0}).".Interpolate(cause),
                        "cause");
            }

            lock (_lock)
            {
                Core.Registers.InterruptLatch = Bits.Set(Core.Registers.InterruptLatch, (int)priority);

                if (cause != ExceptionCause.None)
                    Core.Registers.CoreStatusStore = Bits.Insert(Core.Registers.CoreStatus, (uint)exCause, 16, iv ? 4 : 3);
            }
        }
    }
}
