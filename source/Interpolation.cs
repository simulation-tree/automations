using Unmanaged;

namespace Automations
{
    public unsafe readonly struct Interpolation
    {
        private readonly delegate* unmanaged<Input, void> function;

        public Interpolation(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }

        public readonly void Invoke(Allocation current, Allocation next, float progress, Allocation target, ushort targetByteSize)
        {
            Input input = new(progress, current, next, target, targetByteSize);
            function(input);
        }

        public readonly void Invoke<T>(ref T current, ref T next, float progress, ref T target) where T : unmanaged
        {
            Allocation currentPointer = Allocation.Get(ref current);
            Allocation nextPointer = Allocation.Get(ref next);
            Allocation targetPointer = Allocation.Get(ref target);
            Invoke(currentPointer, nextPointer, progress, targetPointer, (ushort)sizeof(T));
        }

        public readonly struct Input
        {
            public readonly float progress;
            public readonly Allocation current;
            public readonly Allocation next;
            public readonly Allocation component;
            public readonly ushort componentTypeSize;

            public Input(float progress, Allocation current, Allocation next, Allocation component, ushort componentTypeSize)
            {
                this.progress = progress;
                this.current = current;
                this.next = next;
                this.component = component;
                this.componentTypeSize = componentTypeSize;
            }

            public readonly T GetCurrent<T>() where T : unmanaged
            {
                return *(T*)current;
            }

            public readonly T GetNext<T>() where T : unmanaged
            {
                return *(T*)next;
            }

            public readonly void SetComponent<T>(T value) where T : unmanaged
            {
                System.Runtime.CompilerServices.Unsafe.CopyBlock(component, &value, componentTypeSize);
            }
        }
    }
}