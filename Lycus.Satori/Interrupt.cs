namespace Lycus.Satori
{
    public enum Interrupt
    {
        SynchronizeSignal = 0,
        SoftwareException = 1,
        MemoryFault = 2,
        Timer0 = 3,
        Timer1 = 4,
        Message = 5,
        DirectAccess0 = 6,
        DirectAccess1 = 7,
        WiredAnd = 8,
        User = 9,
    }
}
