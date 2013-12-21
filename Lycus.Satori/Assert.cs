using System.Runtime.CompilerServices;

namespace Lycus.Satori
{
    static class Assert
    {
        public static void IsTrue(
            bool condition,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (!condition)
                throw new InternalException("Assertion on {0}:{1} failed.".Interpolate(file, line));
        }

        public static void IsFalse(
            bool condition,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (condition)
                throw new InternalException("Assertion on {0}:{1} failed.".Interpolate(file, line));
        }
    }
}
