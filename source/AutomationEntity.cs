using Automations.Components;
using Collections.Generic;
using System;
using Unmanaged;
using Worlds;
using Array = Collections.Array;

namespace Automations
{
    public readonly partial struct AutomationEntity : IEntity
    {
        public readonly ref bool Loop => ref GetComponent<IsAutomation>().loop;
        public readonly USpan<float> KeyframeTimes => GetArray<KeyframeTime>().AsSpan<float>();
        public readonly Array KeyframeValues => GetArray(KeyframeType);
        public readonly ArrayElementType KeyframeType => GetComponent<IsAutomation>().keyframeType;
        public readonly uint Count => GetArrayLength<KeyframeTime>();

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsAutomation>();
            archetype.AddArrayType<KeyframeTime>();
        }

        /// <summary>
        /// Retrieves the type for the keyframe that can store the given type.
        /// </summary>
        public unsafe static DataType GetKeyframeType<T>(Schema schema) where T : unmanaged
        {
            uint size = (uint)sizeof(T);
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

        public unsafe static ushort GetKeyframeSize<T>() where T : unmanaged
        {
            uint size = (uint)sizeof(T);
            uint containingSize = Allocations.GetNextPowerOf2(size - 1);
            return (ushort)containingSize;
        }
    }

    public readonly struct AutomationEntity<T> : IEntity where T : unmanaged
    {
        public readonly AutomationEntity automation;

        public readonly uint Count => automation.Count;
        public readonly bool IsCompliant => automation.IsCompliant;

        public unsafe readonly (float time, T value) this[uint index]
        {
            get
            {
                Schema schema = automation.world.Schema;
                ArrayElementType keyframeType = AutomationEntity.GetKeyframeType<T>(schema);
                ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
                ref T value = ref automation.GetArray(keyframeType).Items.Read<T>(index * keyframeSize);
                ref float time = ref automation.KeyframeTimes[index];
                return (time, value);
            }
            set
            {
                Schema schema = automation.world.Schema;
                ArrayElementType keyframeType = AutomationEntity.GetKeyframeType<T>(schema);
                ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
                ref T keyframeValue = ref automation.GetArray(keyframeType).Items.Read<T>(index * keyframeSize);
                ref float keyframeTime = ref automation.KeyframeTimes[index];
                keyframeValue = value.value;
                keyframeTime = value.time;
            }
        }

        public readonly ref bool Loop => ref automation.Loop;
        public readonly ArrayElementType KeyframeType => automation.KeyframeType;

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsAutomation>();
            archetype.AddArrayType(AutomationEntity.GetKeyframeType<T>(archetype.schema));
            archetype.AddArrayType<KeyframeTime>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public AutomationEntity()
        {
            throw new NotSupportedException();
        }
#endif
        /// <summary>
        /// Creates an automation with no keyframes.
        /// </summary>
        public AutomationEntity(World world, InterpolationMethod interpolationMethod, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, interpolationMethod, loop));
            world.CreateArray(entity, keyframeType);
            world.CreateArray<KeyframeTime>(entity);
            automation = new Entity(world, entity).As<AutomationEntity>();
        }

        /// <summary>
        /// Creates an automation with no keyframes.
        /// </summary>
        public AutomationEntity(World world, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, default, loop));
            world.CreateArray(entity, keyframeType);
            world.CreateArray<KeyframeTime>(entity);
            automation = new Entity(world, entity).As<AutomationEntity>();
        }

        public AutomationEntity(World world, InterpolationMethod interpolationMethod, USpan<float> times, USpan<T> values, bool loop = false)
        {
            if (values.Length != times.Length)
            {
                throw new ArgumentException($"Values and times given to {nameof(AutomationEntity)} constructor must be the same length");
            }

            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, interpolationMethod, loop));
            Array keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<KeyframeTime>());
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (uint i = 0; i < values.Length; i++)
            {
                keyframeValues.Items.Write(i * keyframeSize, values[i]);
            }
        }

        public AutomationEntity(World world, USpan<float> times, USpan<T> values, bool loop = false)
        {
            if (values.Length != times.Length)
            {
                throw new ArgumentException($"Values and times given to {nameof(AutomationEntity)} constructor must be the same length");
            }

            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, default, loop));
            Array keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<KeyframeTime>());
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (uint i = 0; i < values.Length; i++)
            {
                keyframeValues.Items.Write(i * keyframeSize, values[i]);
            }
        }

        public unsafe AutomationEntity(World world, InterpolationMethod interpolationMethod, USpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, interpolationMethod, loop));
            Array values = world.CreateArray(entity, keyframeType, keyframes.Length);
            Array<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (uint i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Items.Write(i * keyframeSize, keyframes[i].value);
            }
        }

        public unsafe AutomationEntity(World world, USpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, default, loop));
            Array values = world.CreateArray(entity, keyframeType, keyframes.Length);
            Array<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (uint i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Items.Write(i * keyframeSize, keyframes[i].value);
            }
        }

        public readonly void Dispose()
        {
            automation.Dispose();
        }

        public readonly void AddKeyframe(float time, T value)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(automation.world.Schema);
            ref IsAutomation component = ref automation.GetComponent<IsAutomation>();
            component.keyframeType = keyframeType;

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            uint keyframeCount = Count;
            Array values = automation.GetArray(keyframeType);
            values.Length++;
            Array times = automation.GetArray<KeyframeTime>();
            times.Length++;
            values.Set(keyframeCount, value);
            times.Set(keyframeCount, time);
        }

        public static implicit operator AutomationEntity(AutomationEntity<T> automation)
        {
            return automation.automation;
        }

        public static implicit operator Entity(AutomationEntity<T> automation)
        {
            return automation.automation;
        }
    }
}