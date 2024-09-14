using System;

namespace Automations
{
    public struct Keyframe<T> where T : unmanaged
    {
        public float time;
        public uint byteOffset;
        public T value;

#if NET
        [Obsolete("Default constructor not available", true)]
        public Keyframe()
        {
            throw new NotSupportedException();
        }
#endif

        public Keyframe(float time, T value, uint byteOffset = 0)
        {
            this.time = time;
            this.byteOffset = byteOffset;
            this.value = value;
        }
    }
}