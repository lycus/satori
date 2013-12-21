using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Lycus.Satori
{
    public static class Extensions
    {
        public static string Interpolate(this string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            if (args == null)
                throw new ArgumentNullException("args");

            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static float ReinterpretAsSingle(this int value)
        {
            return ((uint)value).ReinterpretAsSingle();
        }

        [CLSCompliant(false)]
        public static unsafe float ReinterpretAsSingle(this uint value)
        {
            return *(float*)&value;
        }

        public static double ReinterpretAsDouble(this long value)
        {
            return ((ulong)value).ReinterpretAsDouble();
        }

        [CLSCompliant(false)]
        public static unsafe double ReinterpretAsDouble(this ulong value)
        {
            return *(double*)&value;
        }

        public static int ReinterpretAsInt32(this float value)
        {
            return (int)value.ReinterpretAsUInt32();
        }

        [CLSCompliant(false)]
        public static unsafe uint ReinterpretAsUInt32(this float value)
        {
            return *(uint*)&value;
        }

        public static long ReinterpretAsInt64(this double value)
        {
            return (long)value.ReinterpretAsUInt64();
        }

        [CLSCompliant(false)]
        public static unsafe ulong ReinterpretAsUInt64(this double value)
        {
            return *(ulong*)&value;
        }

        public static BitArray ToBitArray(this short bits)
        {
            var arr = new BitArray(sizeof(short) * 8);

            // Apparently `BitArray` only speaks 32-bit integers for some
            // incredibly silly reason.
            for (var i = 0; i < sizeof(short) * 8; i++)
                arr[i] = (bits & 1 << i) != 0;

            return arr;
        }

        [CLSCompliant(false)]
        public static BitArray ToBitArray(this ushort bits)
        {
            return ((short)bits).ToBitArray();
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
    }
}
