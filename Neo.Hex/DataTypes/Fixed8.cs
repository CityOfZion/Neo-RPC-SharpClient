﻿using System;
using System.Globalization;
using System.IO;
using Neo.Base.Interfaces;

namespace Neo.Base.DataTypes
{
    /// <summary>
    ///     Accurate to 10 ^ -8 64-bit fixed point, the rounding error can be minimised to a minimum.
    ///     The rounding error can be completely eliminated by controlling the accuracy of the multiplier.
    /// </summary>
    /// <note>Taken from neo-project</note>
    public struct Fixed8 : IComparable<Fixed8>, IEquatable<Fixed8>, IFormattable, ISerializable
    {
        private const long D = 100000000;
        internal long Value;

        public static readonly Fixed8 MaxValue = new Fixed8 {Value = long.MaxValue};

        public static readonly Fixed8 MinValue = new Fixed8 {Value = long.MinValue};

        public static readonly Fixed8 One = new Fixed8 {Value = D};

        public static readonly Fixed8 Satoshi = new Fixed8 {Value = 1};

        public static readonly Fixed8 Zero = default(Fixed8);

        public int Size => sizeof(long);

        public Fixed8(long data)
        {
            Value = data;
        }

        public Fixed8 Abs()
        {
            if (Value >= 0) return this;
            return new Fixed8
            {
                Value = -Value
            };
        }

        public Fixed8 Ceiling()
        {
            var remainder = Value % D;
            if (remainder == 0) return this;
            if (remainder > 0)
                return new Fixed8
                {
                    Value = Value - remainder + D
                };
            return new Fixed8
            {
                Value = Value - remainder
            };
        }

        public int CompareTo(Fixed8 other)
        {
            return Value.CompareTo(other.Value);
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            Value = reader.ReadInt64();
        }

        public bool Equals(Fixed8 other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fixed8)) return false;
            return Equals((Fixed8) obj);
        }

        public static Fixed8 FromDecimal(decimal value)
        {
            value *= D;
            if (value < long.MinValue || value > long.MaxValue)
                throw new OverflowException();
            return new Fixed8
            {
                Value = (long) value
            };
        }

        public long GetData()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static Fixed8 Max(Fixed8 first, params Fixed8[] others)
        {
            foreach (var other in others)
                if (first.CompareTo(other) < 0)
                    first = other;
            return first;
        }

        public static Fixed8 Min(Fixed8 first, params Fixed8[] others)
        {
            foreach (var other in others)
                if (first.CompareTo(other) > 0)
                    first = other;
            return first;
        }

        public static Fixed8 Parse(string s)
        {
            return FromDecimal(decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(Value);
        }

        public override string ToString()
        {
            return ((decimal) this).ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(string format)
        {
            return ((decimal) this).ToString(format);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ((decimal) this).ToString(format, formatProvider);
        }

        public static bool TryParse(string s, out Fixed8 result)
        {
            if (!decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                result = default(Fixed8);
                return false;
            }
            d *= D;
            if (d < long.MinValue || d > long.MaxValue)
            {
                result = default(Fixed8);
                return false;
            }
            result = new Fixed8
            {
                Value = (long) d
            };
            return true;
        }

        public static explicit operator decimal(Fixed8 value)
        {
            return value.Value / (decimal) D;
        }

        public static explicit operator long(Fixed8 value)
        {
            return value.Value / D;
        }

        public static bool operator ==(Fixed8 x, Fixed8 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Fixed8 x, Fixed8 y)
        {
            return !x.Equals(y);
        }

        public static bool operator >(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator >=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) >= 0;
        }

        public static bool operator <=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static Fixed8 operator *(Fixed8 x, Fixed8 y)
        {
            const ulong quo = (1ul << 63) / (D >> 1);
            const ulong rem = (1ul << 63) % (D >> 1);
            var sign = Math.Sign(x.Value) * Math.Sign(y.Value);
            var ux = (ulong) Math.Abs(x.Value);
            var uy = (ulong) Math.Abs(y.Value);
            var xh = ux >> 32;
            var xl = ux & 0x00000000fffffffful;
            var yh = uy >> 32;
            var yl = uy & 0x00000000fffffffful;
            var rh = xh * yh;
            var rm = xh * yl + xl * yh;
            var rl = xl * yl;
            var rmh = rm >> 32;
            var rml = rm << 32;
            rh += rmh;
            rl += rml;
            if (rl < rml)
                ++rh;
            if (rh >= D)
                throw new OverflowException();
            var r = rh * quo + (rh * rem + rl) / D;
            x.Value = (long) r * sign;
            return x;
        }

        public static Fixed8 operator *(Fixed8 x, long y)
        {
            x.Value *= y;
            return x;
        }

        public static Fixed8 operator /(Fixed8 x, long y)
        {
            x.Value /= y;
            return x;
        }

        public static Fixed8 operator +(Fixed8 x, Fixed8 y)
        {
            x.Value = checked(x.Value + y.Value);
            return x;
        }

        public static Fixed8 operator -(Fixed8 x, Fixed8 y)
        {
            x.Value = checked(x.Value - y.Value);
            return x;
        }

        public static Fixed8 operator -(Fixed8 value)
        {
            value.Value = -value.Value;
            return value;
        }
    }
}