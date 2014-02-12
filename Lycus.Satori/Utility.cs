using System;

namespace Lycus.Satori
{
    static class Utility
    {
        public static void Ignore<T>(T value)
        {
        }

        public static bool CheckedAdd(int a, int b)
        {
            try
            {
                Ignore(checked(a + b));
            }
            catch (OverflowException)
            {
                return true;
            }

            return false;
        }

        public static bool CheckedSubtract(int a, int b)
        {
            try
            {
                Ignore(checked(a - b));
            }
            catch (OverflowException)
            {
                return true;
            }

            return false;
        }

        internal static string StringizeRegister(RegisterGroup? group, int register)
        {
            if (group == null)
            {
                if (register < RegisterFile.MinRegister || register > RegisterFile.MaxRegister)
                    throw new ArgumentOutOfRangeException("register", register,
                        "General-purpose register number is out of range.");

                return "r" + register;
            }

            switch ((RegisterGroup)group)
            {
                case RegisterGroup.CoreControl:
                    switch (register)
                    {
                        case 0:
                            return "config";
                        case 1:
                            return "status";
                        case 2:
                            return "pc";
                        case 3:
                            return "debugstatus";
                        case 5:
                            return "lc";
                        case 6:
                            return "ls";
                        case 7:
                            return "le";
                        case 8:
                            return "iret";
                        case 9:
                            return "imask";
                        case 10:
                            return "ilat";
                        case 11:
                            return "ilatst";
                        case 12:
                            return "ilatcl";
                        case 13:
                            return "ipend";
                        case 14:
                            return "ctimer0";
                        case 15:
                            return "ctimer1";
                        case 16:
                            return "fstatus";
                        case 17:
                            return "debugcmd";
                    }

                    break;
                case RegisterGroup.DirectAccess:
                    switch (register)
                    {
                        case 0:
                            return "dma0config";
                        case 1:
                            return "dma0stride";
                        case 2:
                            return "dma0count";
                        case 3:
                            return "dma0srcaddr";
                        case 4:
                            return "dma0dstaddr";
                        case 5:
                            return "dma0auto0";
                        case 6:
                            return "dma0auto1";
                        case 7:
                            return "dma0status";
                        case 8:
                            return "dma1config";
                        case 9:
                            return "dma1stride";
                        case 10:
                            return "dma1count";
                        case 11:
                            return "dma1srcaddr";
                        case 12:
                            return "dma1dstaddr";
                        case 13:
                            return "dma1auto0";
                        case 14:
                            return "dma1auto1";
                        case 15:
                            return "dma1status";
                    }

                    break;
                case RegisterGroup.MemoryProtection:
                    switch (register)
                    {
                        case 1:
                            return "memstatus";
                        case 2:
                            return "memprotect";
                    }

                    break;
                case RegisterGroup.NodeConfiguration:
                    switch (register)
                    {
                        case 0:
                            return "meshconfig";
                        case 1:
                            return "coreid";
                        case 2:
                            return "multicast";
                        case 3:
                            return "resetcore";
                        case 4:
                            return "cmeshroute";
                        case 5:
                            return "xmeshroute";
                        case 6:
                            return "rmeshroute";
                    }

                    break;
            }

            throw new ArgumentException(
                "Invalid system register value ({0}).".Interpolate(register),
                "register");
        }
    }
}
