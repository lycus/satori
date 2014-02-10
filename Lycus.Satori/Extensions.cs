using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace Lycus.Satori
{
    public static class Extensions
    {
        [StringFormatMethod("format")]
        public static string Interpolate(this string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static float CoerceToSingle(this int value)
        {
            return ((uint)value).CoerceToSingle();
        }

        [CLSCompliant(false)]
        public static unsafe float CoerceToSingle(this uint value)
        {
            return *(float*)&value;
        }

        public static double CoerceToDouble(this long value)
        {
            return ((ulong)value).CoerceToDouble();
        }

        [CLSCompliant(false)]
        public static unsafe double CoerceToDouble(this ulong value)
        {
            return *(double*)&value;
        }

        public static int CoerceToInt32(this float value)
        {
            return (int)value.CoerceToUInt32();
        }

        [CLSCompliant(false)]
        public static unsafe uint CoerceToUInt32(this float value)
        {
            return *(uint*)&value;
        }

        public static long CoerceToInt64(this double value)
        {
            return (long)value.CoerceToUInt64();
        }

        [CLSCompliant(false)]
        public static unsafe ulong CoerceToUInt64(this double value)
        {
            return *(ulong*)&value;
        }

        public static BitArray ToBitArray(this int bits)
        {
            return new BitArray(new[] { bits });
        }

        [CLSCompliant(false)]
        public static BitArray ToBitArray(this uint bits)
        {
            return ((int)bits).ToBitArray();
        }

        public static int ToInt32(this BitArray bits)
        {
            if (bits == null)
                throw new ArgumentNullException("bits");

            if (bits.Length > 32)
                throw new ArgumentException("Bit array length must be 32 or less.", "bits");

            var arr = new int[1];

            bits.CopyTo(arr, 0);

            return arr[0];
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(this BitArray bits)
        {
            if (bits == null)
                throw new ArgumentNullException("bits");

            if (bits.Length > 32)
                throw new ArgumentException("Bit array length must be 32 or less.", "bits");

            return (uint)bits.ToInt32();
        }

        public static string ToBitString(this BitArray bits)
        {
            if (bits == null)
                throw new ArgumentNullException("bits");

            var sb = new StringBuilder(bits.Count);

            for (var i = bits.Count - 1; i >= 0; i--)
                sb.Append(bits[i] ? "1" : "0");

            return sb.ToString();
        }

        public static string ToAssemblyString(this ConditionCode condition)
        {
            switch (condition)
            {
                case ConditionCode.Equal:
                    return "eq";
                case ConditionCode.NotEqual:
                    return "ne";
                case ConditionCode.UnsignedGreaterThan:
                    return "gtu";
                case ConditionCode.UnsignedGreaterThanOrEqual:
                    return "gteu";
                case ConditionCode.UnsignedLessThanOrEqual:
                    return "lteu";
                case ConditionCode.UnsignedLessThan:
                    return "ltu";
                case ConditionCode.SignedGreaterThan:
                    return "gt";
                case ConditionCode.SignedGreaterThanOrEqual:
                    return "gte";
                case ConditionCode.SignedLessThan:
                    return "lt";
                case ConditionCode.SignedLessThanOrEqual:
                    return "lte";
                case ConditionCode.FloatEqual:
                    return "beq";
                case ConditionCode.FloatNotEqual:
                    return "bne";
                case ConditionCode.FloatLessThan:
                    return "blt";
                case ConditionCode.FloatLessThanOrEqual:
                    return "blte";
                case ConditionCode.Unconditional:
                    return string.Empty;
                case ConditionCode.BranchAndLink:
                    return "l";
                default:
                    throw new ArgumentException(
                        "Invalid ConditionCode value ({0}).".Interpolate(condition),
                        "condition");
            }
        }

        public static string ToAssemblyString(this Size size)
        {
            switch (size)
            {
                case Size.Int8:
                    return "b";
                case Size.Int16:
                    return "h";
                case Size.Int32:
                    return string.Empty;
                case Size.Int64:
                    return "d";
                default:
                    throw new ArgumentException(
                        "Invalid Size value ({0}).".Interpolate(size),
                        "size");
            }
        }

        public static int ExtractMantissa(this float value)
        {
            return Bits.Extract(value.CoerceToInt32(), 0, 22);
        }

        public static int ExtractExponent(this float value)
        {
            return Bits.Extract(value.CoerceToInt32(), 22, 8);
        }

        public static int ExtractUnbiasedExponent(this float value)
        {
            return value.ExtractExponent() - 127;
        }

        public static bool ExtractSign(this float value)
        {
            return Bits.Check(value.CoerceToInt32(), 31);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Denormal")]
        public static bool IsDenormal(this float value)
        {
            return value.ExtractExponent() == 0 && value.ExtractMantissa() != 0;
        }

        public static bool IsNegative(this float value)
        {
            return Bits.Check(value.CoerceToInt32(), 31);
        }

        public static float ToPositive(this float value)
        {
            return Bits.Clear(value.CoerceToInt32(), 31).CoerceToSingle();
        }

        public static float ToNegative(this float value)
        {
            return Bits.Set(value.CoerceToInt32(), 31).CoerceToSingle();
        }

        public static float ToZero(this float value)
        {
            return Bits.Insert(value.CoerceToInt32(), 0, 0, 31).CoerceToSingle();
        }

        public static int SignExtend(this int value, int bits)
        {
            if (bits < 1 || bits > sizeof(int) * 8 - 1)
                throw new ArgumentOutOfRangeException("bits", bits,
                    "Number of bits must be in the range 1 to {0}.".Interpolate(sizeof(int) * 8 - 1));

            if (!Bits.Check(value, bits - 1))
                return value;

            var mask = 1 << bits - 1;
            var value2 = value & (1 << bits) - 1;

            return (value2 ^ mask) - mask;
        }
    }
}
