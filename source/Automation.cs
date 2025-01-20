using Automations.Components;
using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly struct Automation : IEntity
    {
        private readonly Entity entity;

        public readonly ref bool Loop => ref entity.GetComponent<IsAutomation>().loop;
        public readonly USpan<float> KeyframeTimes => entity.GetArray<KeyframeTime>().As<float>();
        public readonly Allocation KeyframeValues => entity.GetArray(KeyframeType);
        public readonly ArrayElementType KeyframeType => entity.GetComponent<IsAutomation>().keyframeType;
        public readonly uint Count => entity.GetArrayLength<KeyframeTime>();

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsAutomation>();
            archetype.AddArrayElementType<KeyframeTime>();
        }

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
        public static DataType GetKeyframeType<T>(Schema schema) where T : unmanaged
        {
            uint size = TypeInfo<T>.size;
            uint containingSize = Allocations.GetNextPowerOf2(size - 1);
            uint index = Allocations.GetIndexOfPowerOf2(containingSize);
            return index switch
            {
                0 => schema.GetArrayElementDataType<KeyframeValue1>(),
                1 => schema.GetArrayElementDataType<KeyframeValue2>(),
                2 => schema.GetArrayElementDataType<KeyframeValue4>(),
                3 => schema.GetArrayElementDataType<KeyframeValue8>(),
                4 => schema.GetArrayElementDataType<KeyframeValue16>(),
                5 => schema.GetArrayElementDataType<KeyframeValue32>(),
                6 => schema.GetArrayElementDataType<KeyframeValue64>(),
                7 => schema.GetArrayElementDataType<KeyframeValue128>(),
                8 => schema.GetArrayElementDataType<KeyframeValue256>(),
                _ => throw new NotSupportedException($"Keyframe value type `{typeof(T)}` is greater than the maximum 256")
            };
        }

        public static ushort GetKeyframeSize<T>() where T : unmanaged
        {
            uint size = TypeInfo<T>.size;
            uint containingSize = Allocations.GetNextPowerOf2(size - 1);
            return (ushort)containingSize;
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
                Schema schema = automation.GetWorld().Schema;
                ArrayElementType keyframeType = Automation.GetKeyframeType<T>(schema);
                ushort keyframeSize = Automation.GetKeyframeSize<T>();
                ref T value = ref automation.AsEntity().GetArray(keyframeType).Read<T>(index * keyframeSize);
                ref float time = ref automation.KeyframeTimes[index];
                return (time, value);
            }
            set
            {
                Schema schema = automation.GetWorld().Schema;
                ArrayElementType keyframeType = Automation.GetKeyframeType<T>(schema);
                ushort keyframeSize = Automation.GetKeyframeSize<T>();
                ref T keyframeValue = ref automation.AsEntity().GetArray(keyframeType).Read<T>(index * keyframeSize);
                ref float keyframeTime = ref automation.KeyframeTimes[index];
                keyframeValue = value.value;
                keyframeTime = value.time;
            }
        }

        public readonly ref bool Loop => ref automation.Loop;
        public readonly ArrayElementType KeyframeType => automation.KeyframeType;

        readonly uint IEntity.Value => automation.GetEntityValue();
        readonly World IEntity.World => automation.GetWorld();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsAutomation>();
            archetype.AddArrayElementType(Automation.GetKeyframeType<T>(archetype.schema));
            archetype.AddArrayElementType<KeyframeTime>();
        }

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
            DataType keyframeType = Automation.GetKeyframeType<T>(world.Schema);
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
            DataType keyframeType = Automation.GetKeyframeType<T>(world.Schema);
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

            DataType keyframeType = Automation.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, interpolationMethod, loop));
            Allocation keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<KeyframeTime>());
            automation = new(world, entity);

            ushort keyframeSize = Automation.GetKeyframeSize<T>();
            for (uint i = 0; i < values.Length; i++)
            {
                keyframeValues.Write(i * keyframeSize, values[i]);
            }
        }

        public unsafe Automation(World world, USpan<float> times, USpan<T> values, bool loop = false)
        {
            if (values.Length != times.Length)
            {
                throw new ArgumentException($"Values and times given to {nameof(Automation)} constructor must be the same length");
            }

            DataType keyframeType = Automation.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, default, loop));
            Allocation keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<KeyframeTime>());
            automation = new(world, entity);

            ushort keyframeSize = Automation.GetKeyframeSize<T>();
            for (uint i = 0; i < values.Length; i++)
            {
                keyframeValues.Write(i * keyframeSize, values[i]);
            }
        }

        public unsafe Automation(World world, InterpolationMethod interpolationMethod, USpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = Automation.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, interpolationMethod, loop));
            Allocation values = world.CreateArray(entity, keyframeType, keyframes.Length);
            USpan<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new(world, entity);

            ushort keyframeSize = Automation.GetKeyframeSize<T>();
            for (uint i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Write(i * keyframeSize, keyframes[i].value);
            }
        }

        public unsafe Automation(World world, USpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = Automation.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(keyframeType, default, loop));
            Allocation values = world.CreateArray(entity, keyframeType, keyframes.Length);
            USpan<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new(world, entity);

            ushort keyframeSize = Automation.GetKeyframeSize<T>();
            for (uint i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Write(i * keyframeSize, keyframes[i].value);
            }
        }

        public readonly void Dispose()
        {
            automation.Dispose();
        }

        public readonly void AddKeyframe(float time, T value)
        {
            DataType keyframeType = Automation.GetKeyframeType<T>(automation.GetWorld().Schema);
            ref IsAutomation component = ref automation.AsEntity().GetComponent<IsAutomation>();
            component.keyframeType = keyframeType;

            ushort keyframeSize = Automation.GetKeyframeSize<T>();
            uint keyframeCount = Count;
            Allocation values = automation.AsEntity().ResizeArray(keyframeType, keyframeCount + 1);
            USpan<KeyframeTime> times = automation.AsEntity().ResizeArray<KeyframeTime>(keyframeCount + 1);
            values.Write(keyframeCount * keyframeSize, value);
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