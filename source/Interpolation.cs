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

        public readonly void Invoke(void* current, void* next, float progress, void* component, ushort componentTypeSize)
        {
            Input input = new(progress, current, next, component, componentTypeSize);
            function(input);
        }

        public readonly void Invoke<T>(ref T current, ref T next, float progress, ref T component) where T : unmanaged
        {
            void* currentPointer = System.Runtime.CompilerServices.Unsafe.AsPointer(ref current);
            void* nextPointer = System.Runtime.CompilerServices.Unsafe.AsPointer(ref next);
            void* componentPointer = System.Runtime.CompilerServices.Unsafe.AsPointer(ref component);
            Invoke(currentPointer, nextPointer, progress, componentPointer, (ushort)TypeInfo<T>.size);
        }

        public readonly struct Input
        {
            public readonly float progress;
            public readonly void* current;
            public readonly void* next;
            public readonly void* component;
            public readonly ushort componentTypeSize;

            public Input(float progress, void* current, void* next, void* component, ushort componentTypeSize)
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