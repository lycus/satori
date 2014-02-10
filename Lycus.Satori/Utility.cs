using System;

namespace Lycus.Satori
{
    static class Utility
    {
        public static void Ignore<T>(T t)
        {
        }

        public static bool CheckedAdd(int a, int b)
        {
            try
            {
                Utility.Ignore(a + b);
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
                Utility.Ignore(a - b);
            }
            catch (OverflowException)
            {
                return true;
            }

            return false;
        }
    }
}
