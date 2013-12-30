using System;

namespace Lycus.Satori
{
    /// <summary>
    /// Provides utilities for operating on bits of integers.
    /// </summary>
    public static class Bits
    {
        static void CheckIndex(int size, int bit, string name = "bit")
        {
            if (bit > size * 8 - 1)
                throw new ArgumentOutOfRangeException(name, bit, "Bit index is too high.");
        }

        static void CheckRange(int size, int start, int count)
        {
            CheckIndex(size, start, "start");

            if (start + count > size * 8)
                throw new ArgumentException("Bit range is out of bounds.");
        }

        public static int Insert(int value1, int value2, int start, int count)
        {
            CheckRange(sizeof(int), start, count);

            var mask = (1 << count) - 1;

            return value1 & ~(mask << start) | (value2 & mask) << start;
        }

        [CLSCompliant(false)]
        public static uint Insert(uint value1, uint value2, int start, int count)
        {
            CheckRange(sizeof(uint), start, count);

            var mask = (1u << count) - 1;

            return value1 & ~(mask << start) | (value2 & mask) << start;
        }

        public static long Insert(long value1, long value2, int start, int count)
        {
            CheckRange(sizeof(long), start, count);

            var mask = (1 << count) - 1;

            return value1 & ~(mask << start) | (value2 & mask) << start;
        }

        [CLSCompliant(false)]
        public static ulong Insert(ulong value1, ulong value2, int start, int count)
        {
            CheckRange(sizeof(ulong), start, count);

            var mask = (1ul << count) - 1;

            return value1 & ~(mask << start) | (value2 & mask) << start;
        }

        public static int Extract(int value, int start, int count)
        {
            CheckRange(sizeof(int), start, count);

            return value >> start & (1 << count) - 1;
        }

        [CLSCompliant(false)]
        public static uint Extract(uint value, int start, int count)
        {
            CheckRange(sizeof(uint), start, count);

            return value >> start & (1u << count) - 1;
        }

        public static long Extract(long value, int start, int count)
        {
            CheckRange(sizeof(long), start, count);

            return value >> start & (1 << count) - 1;
        }

        [CLSCompliant(false)]
        public static ulong Extract(ulong value, int start, int count)
        {
            CheckRange(sizeof(ulong), start, count);

            return value >> start & (1u << count) - 1;
        }

        public static int Clear(int value, int bit)
        {
            CheckIndex(sizeof(int), bit);

            return value & ~(1 << bit);
        }

        [CLSCompliant(false)]
        public static uint Clear(uint value, int bit)
        {
            CheckIndex(sizeof(uint), bit);

            return value & ~(1u << bit);
        }

        public static long Clear(long value, int bit)
        {
            CheckIndex(sizeof(long), bit);

            return value & ~(1 << bit);
        }

        [CLSCompliant(false)]
        public static ulong Clear(ulong value, int bit)
        {
            CheckIndex(sizeof(ulong), bit);

            return value & ~(1ul << bit);
        }

        public static int Set(int value, int bit)
        {
            CheckIndex(sizeof(int), bit);

            return value | (1 << bit);
        }

        [CLSCompliant(false)]
        public static uint Set(uint value, int bit)
        {
            CheckIndex(sizeof(uint), bit);

            return value | (1u << bit);
        }

        public static long Set(long value, int bit)
        {
            CheckIndex(sizeof(long), bit);

            return value | (1l << bit);
        }

        [CLSCompliant(false)]
        public static ulong Set(ulong value, int bit)
        {
            CheckIndex(sizeof(ulong), bit);

            return value | (1ul << bit);
        }

        public static int Toggle(int value, int bit)
        {
            CheckIndex(sizeof(int), bit);

            return value ^ 1 << bit;
        }

        [CLSCompliant(false)]
        public static uint Toggle(uint value, int bit)
        {
            CheckIndex(sizeof(uint), bit);

            return value ^ 1u << bit;
        }

        public static long Toggle(long value, int bit)
        {
            CheckIndex(sizeof(long), bit);

            return value ^ 1 << bit;
        }

        [CLSCompliant(false)]
        public static ulong Toggle(ulong value, int bit)
        {
            CheckIndex(sizeof(ulong), bit);

            return value ^ 1ul << bit;
        }

        public static bool Check(int value, int bit)
        {
            CheckIndex(sizeof(int), bit);

            return (value & 1 << bit) != 0;
        }

        [CLSCompliant(false)]
        public static bool Check(uint value, int bit)
        {
            CheckIndex(sizeof(uint), bit);

            return (value & 1 << bit) != 0;
        }

        public static bool Check(long value, int bit)
        {
            CheckIndex(sizeof(long), bit);

            return (value & 1 << bit) != 0;
        }

        [CLSCompliant(false)]
        public static bool Check(ulong value, int bit)
        {
            CheckIndex(sizeof(ulong), bit);

            return (value & 1ul << bit) != 0;
        }
    }
}
