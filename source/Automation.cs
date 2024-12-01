using Automations.Components;
using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly struct Automation : IEntity
    {
        private static readonly ArrayType[] keyframeTypes;

        static Automation()
        {
            keyframeTypes = [
                ArrayType.Get<KeyframeValue1>(),
                ArrayType.Get<KeyframeValue2>(),
                ArrayType.Get<KeyframeValue4>(),
                ArrayType.Get<KeyframeValue8>(),
                ArrayType.Get<KeyframeValue16>(),
                ArrayType.Get<KeyframeValue32>(),
                ArrayType.Get<KeyframeValue64>(),
                ArrayType.Get<KeyframeValue128>(),
                ArrayType.Get<KeyframeValue256>()
            ];
        }

        private readonly Entity entity;

        public readonly ref bool Loop => ref entity.GetComponentRef<IsAutomation>().loop;
        public readonly USpan<float> KeyframeTimes => entity.GetArray<KeyframeTime>().As<float>();
        public readonly Allocation KeyframeValues => entity.GetArray(KeyframeType);
        public readonly ArrayType KeyframeType => entity.GetComponentRef<IsAutomation>().keyframeType;
        public readonly uint Count => entity.GetArrayLength<KeyframeTime>();

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsAutomation>().AddArrayType<KeyframeTime>();

#if NET
        [Obsolete("Default constructor not available", true)]
        public Automation()
        {
            throw new NotSupportedException();
        }
#endif

        public Automation(World world, uint existingEntity)
        {
            this.entity = new(world, existingEntity);
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public static implicit operator Entity(Automation automation)
        {
            return automation.entity;
        }

        /// <summary>
        /// Retrieves the type for the keyframe that can store the given type.
        /// </summary>
        public static ArrayType GetKeyframeType<T>() where T : unmanaged
        {
            uint size = TypeInfo<T>.size;
            uint containingSize = Allocations.GetNextPowerOf2(size - 1);
            uint index = Allocations.GetIndexOfPowerOf2(containingSize);
            if (index > keyframeTypes.Length)
            {
                throw new NotSupportedException($"Keyframe value type `{typeof(T)}` is greater than the maximum {keyframeTypes[^1].Size}");
            }

            return keyframeTypes[index];
        }
    }

    public readonly struct Automation<T> : IEntity where T : unmanaged
    {
        public readonly Automation automation;

        public readonly uint Count => automation.Count;

        public unsafe readonly (float time, T value) this[uint index]
        {
            get
            {
                ArrayType keyframeType = Automation.GetKeyframeType<T>();
                ref T value = ref automation.AsEntity().GetArray(keyframeType).Read<T>(index * keyframeType.Size);
                ref float time = ref automation.KeyframeTimes[index];
                return (time, value);
            }
            set
            {
                ArrayType keyframeType = Automation.GetKeyframeType<T>();
                ref T keyframeValue = ref automation.AsEntity().GetArray(keyframeType).Read<T>(index * keyframeType.Size);
                ref float keyframeTime = ref automation.KeyframeTimes[index];
                keyframeValue = value.value;
                keyframeTime = value.time;
            }
        }

        public readonly ref bool Loop => ref automation.Loop;
        public readonly ArrayType KeyframeType => automation.KeyframeType;

        readonly uint IEntity.Value => automation.GetEntityValue();
        readonly World IEntity.World => automation.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsAutomation>().AddArrayType(Automation.GetKeyframeType<T>()).AddArrayType<KeyframeTime>();

#if NET
        [Obsolete("Default constructor not available", true)]
        public Automation()
        {
            throw new NotSupportedException();
        }
#endif
        /// <summary>
        /// Creates an automation with no keyframes.
        /// </summary>
        public Automation(World world, InterpolationMethod interpolationMethod, bool loop = false)
        {
            ArrayType keyframeType = Automation.GetKeyframeType<T>();
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, interpolationMethod, loop));
            world.CreateArray(entity, keyframeType);
            world.CreateArray<KeyframeTime>(entity);
            automation = new(world, entity);
        }

        /// <summary>
        /// Creates an automation with no keyframes.
        /// </summary>
        public Automation(World world, bool loop = false)
        {
            ArrayType keyframeType = Automation.GetKeyframeType<T>();
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, default, loop));
            world.CreateArray(entity, keyframeType);
            world.CreateArray<KeyframeTime>(entity);
            automation = new(world, entity);
        }

        public unsafe Automation(World world, InterpolationMethod interpolationMethod, USpan<float> times, USpan<T> values, bool loop = false)
        {
            if (values.Length != times.Length)
            {
                throw new ArgumentException($"Values and times given to {nameof(Automation)} constructor must be the same length");
            }

            ArrayType keyframeType = Automation.GetKeyframeType<T>();
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, interpolationMethod, loop));
            Allocation keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<KeyframeTime>());
            automation = new(world, entity);

            for (uint i = 0; i < values.Length; i++)
            {
                keyframeValues.Write(i * keyframeType.Size, values[i]);
            }
        }

        public unsafe Automation(World world, USpan<float> times, USpan<T> values, bool loop = false)
        {
            if (values.Length != times.Length)
            {
                throw new ArgumentException($"Values and times given to {nameof(Automation)} constructor must be the same length");
            }

            ArrayType keyframeType = Automation.GetKeyframeType<T>();
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, default, loop));
            Allocation keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<KeyframeTime>());
            automation = new(world, entity);

            for (uint i = 0; i < values.Length; i++)
            {
                keyframeValues.Write(i * keyframeType.Size, values[i]);
            }
        }

        public unsafe Automation(World world, InterpolationMethod interpolationMethod, USpan<(float time, T value)> keyframes, bool loop = false)
        {
            ArrayType keyframeType = Automation.GetKeyframeType<T>();
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, interpolationMethod, loop));
            Allocation values = world.CreateArray(entity, keyframeType, keyframes.Length);
            USpan<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new(world, entity);

            for (uint i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Write(i * keyframeType.Size, keyframes[i].value);
            }
        }

        public unsafe Automation(World world, USpan<(float time, T value)> keyframes, bool loop = false)
        {
            ArrayType keyframeType = Automation.GetKeyframeType<T>();
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, default, loop));
            Allocation values = world.CreateArray(entity, keyframeType, keyframes.Length);
            USpan<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new(world, entity);

            for (uint i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Write(i * keyframeType.Size, keyframes[i].value);
            }
        }

        public readonly void Dispose()
        {
            automation.Dispose();
        }

        public readonly void AddKeyframe(float time, T value)
        {
            ArrayType keyframeType = Automation.GetKeyframeType<T>();
            ref IsAutomation component = ref automation.AsEntity().GetComponentRef<IsAutomation>();
            component.keyframeType = keyframeType;

            uint keyframeCount = Count;
            Allocation values = automation.AsEntity().ResizeArray(keyframeType, keyframeCount + 1);
            USpan<KeyframeTime> times = automation.AsEntity().ResizeArray<KeyframeTime>(keyframeCount + 1);
            values.Write(keyframeCount * keyframeType.Size, value);
            times[keyframeCount] = time;
        }

        public static implicit operator Automation(Automation<T> automation)
        {
            return automation.automation;
        }

        public static implicit operator Entity(Automation<T> automation)
        {
            return automation.automation;
        }
    }
}