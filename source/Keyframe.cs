using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    [Array]
    [Obsolete]
    public struct Keyframe<T> where T : unmanaged
    {
        public float time;
        public T value;

#if NET
        [Obsolete("Default constructor not available", true)]
        public Keyframe()
        {
            throw new NotSupportedException();
        }
#endif

        public Keyframe(float time, T value)
        {
            this.time = time;
            this.value = value;
        }
    }

    [Array]
    public struct KeyframeValue1
    {
        public byte value;

        public KeyframeValue1(byte value)
        {
            this.value = value;
        }
    }

    [Array]
    public struct KeyframeValue2
    {
        public ushort value;

        public KeyframeValue2(ushort value)
        {
            this.value = value;
        }
    }

    [Array]
    public struct KeyframeValue4
    {
        public uint value;

        public KeyframeValue4(uint value)
        {
            this.value = value;
        }
    }

    [Array]
    public struct KeyframeValue8
    {
        public ulong value;

        public KeyframeValue8(ulong value)
        {
            this.value = value;
        }
    }

    [Array]
    public struct KeyframeValue16
    {
        public ulong a;
        public ulong b;

        public KeyframeValue16(ulong a, ulong b)
        {
            this.a = a;
            this.b = b;
        }
    }

    [Array]
    public struct KeyframeValue256
    {
        public unsafe fixed byte value[256];

        public unsafe KeyframeValue256(USpan<byte> value)
        {
            fixed (byte* ptr = this.value)
            {
                for (uint i = 0; i < value.Length; i++)
                {
                    ptr[i] = value[i];
                }
            }
        }
    }

    [Array]
    public struct KeyframeTime
    {
        public float time;
    }

    public unsafe readonly struct KeyframeInterpolator
    {
        private readonly delegate* unmanaged<Input, Output> function;

        public KeyframeInterpolator(delegate* unmanaged<Input, Output> function)
        {
            this.function = function;
        }

        public readonly T Invoke<T>(T current, T next, float progress)
        {

        }

        public readonly struct Input
        {

        }

        public readonly struct Output
        {

        }
    }
}