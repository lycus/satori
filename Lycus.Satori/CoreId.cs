using System;

namespace Lycus.Satori
{
    /// <summary>
    /// Identifies an Epiphany eCore by row and column.
    /// </summary>
    public struct CoreId : IEquatable<CoreId>
    {
        public const int MinRow = 0;

        public const int MaxRow = 63;

        public const int MinColumn = 0;

        public const int MaxColumn = 63;

        public static readonly CoreId Current = new CoreId(0, 0);

        public int Row { get; private set; }

        public int Column { get; private set; }

        public CoreId(int row, int column)
            : this()
        {
            if (row < MinRow || row > MaxRow)
                throw new ArgumentOutOfRangeException("row", row,
                    "Row is out of range.");

            if (column < MinColumn || column > MaxColumn)
                throw new ArgumentOutOfRangeException("column", column,
                    "Column is out of range.");

            Row = row;
            Column = column;
        }

        [CLSCompliant(false)]
        public static CoreId FromAddress(uint address)
        {
            uint raw;

            return FromAddress(address, out raw);
        }

        [CLSCompliant(false)]
        public static CoreId FromAddress(uint address, out uint raw)
        {
            raw = address & ~0xFFF00000;

            var id = address >> 20;

            return new CoreId((int)(id >> 6), (int)(id & 63));
        }

        [CLSCompliant(false)]
        public uint ToAddress(uint raw = 0)
        {
            return raw | ((uint)Row << 26) | ((uint)Column << 20);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is CoreId))
                return false;

            return Equals((CoreId)obj);
        }

        public bool Equals(CoreId other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override int GetHashCode()
        {
            var hash = 17;

            hash = 23 * hash + Row.GetHashCode();
            hash = 23 * hash + Column.GetHashCode();

            return hash;
        }

        public static bool operator ==(CoreId left, CoreId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CoreId left, CoreId right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "({0}, {1})".Interpolate(Row, Column);
        }
    }
}
