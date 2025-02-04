using System;

namespace Automations
{
    public readonly struct InterpolationMethod : IEquatable<InterpolationMethod>
    {
        public static readonly InterpolationMethod FloatLinear = new(1);
        public static readonly InterpolationMethod Vector2Linear = new(2);
        public static readonly InterpolationMethod Vector3Linear = new(3);
        public static readonly InterpolationMethod Vector4Linear = new(4);

        public readonly byte value;

        public InterpolationMethod(byte value)
        {
            this.value = value;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is InterpolationMethod method && Equals(method);
        }

        public readonly bool Equals(InterpolationMethod other)
        {
            return value == other.value;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(value);
        }

        public static bool operator ==(InterpolationMethod left, InterpolationMethod right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InterpolationMethod left, InterpolationMethod right)
        {
            return !(left == right);
        }

        public static implicit operator byte(InterpolationMethod method)
        {
            if (method.value == default)
            {
                throw new InvalidOperationException("InterpolationMethod is not set");
            }

            return (byte)(method.value - 1);
        }
    }
}