using Automations.Components;
using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly partial struct AutomationEntity : IEntity
    {
        public readonly ref bool Loop => ref GetComponent<IsAutomation>().loop;
        public readonly System.Span<float> KeyframeTimes => GetArray<KeyframeTime>().AsSpan<float>();
        public readonly Values KeyframeValues => GetArray(KeyframeType);
        public readonly ArrayElementType KeyframeType => GetComponent<IsAutomation>().keyframeType.ArrayType;
        public readonly int Count => GetArrayLength<KeyframeTime>();

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
            uint containingSize = (size - 1).GetNextPowerOf2();
            uint index = containingSize.GetIndexOfPowerOf2();
            return index switch
            {
                0 => new(schema.GetArrayType<KeyframeValue1>(), 1),
                1 => new(schema.GetArrayType<KeyframeValue2>(), 2),
                2 => new(schema.GetArrayType<KeyframeValue4>(), 4),
                3 => new(schema.GetArrayType<KeyframeValue8>(), 8),
                4 => new(schema.GetArrayType<KeyframeValue16>(), 16),
                5 => new(schema.GetArrayType<KeyframeValue32>(), 32),
                6 => new(schema.GetArrayType<KeyframeValue64>(), 64),
                7 => new(schema.GetArrayType<KeyframeValue128>(), 128),
                8 => new(schema.GetArrayType<KeyframeValue256>(), 256),
                _ => throw new NotSupportedException($"Keyframe value type `{typeof(T)}` is greater than the maximum 256")
            };
        }

        public unsafe static ushort GetKeyframeSize<T>() where T : unmanaged
        {
            uint size = (uint)sizeof(T);
            uint containingSize = (size - 1).GetNextPowerOf2();
            return (ushort)containingSize;
        }
    }

    public readonly struct AutomationEntity<T> : IEntity where T : unmanaged
    {
        public readonly AutomationEntity automation;

        public readonly int Count => automation.Count;
        public readonly bool IsCompliant => automation.IsCompliant;

        public unsafe readonly (float time, T value) this[int index]
        {
            get
            {
                Schema schema = automation.world.Schema;
                ArrayElementType keyframeType = AutomationEntity.GetKeyframeType<T>(schema).ArrayType;
                ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
                ref T value = ref automation.GetArray(keyframeType).Read<T>(index * keyframeSize);
                ref float time = ref automation.KeyframeTimes[index];
                return (time, value);
            }
            set
            {
                Schema schema = automation.world.Schema;
                ArrayElementType keyframeType = AutomationEntity.GetKeyframeType<T>(schema).ArrayType;
                ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
                ref T keyframeValue = ref automation.GetArray(keyframeType).Read<T>(index * keyframeSize);
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
            archetype.AddArrayType(AutomationEntity.GetKeyframeType<T>(archetype.schema).ArrayType);
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
            world.CreateArray(entity, keyframeType.ArrayType);
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
            world.CreateArray(entity, keyframeType.ArrayType);
            world.CreateArray<KeyframeTime>(entity);
            automation = new Entity(world, entity).As<AutomationEntity>();
        }

        public AutomationEntity(World world, InterpolationMethod interpolationMethod, ReadOnlySpan<float> times, ReadOnlySpan<T> values, bool loop = false)
        {
            if (values.Length != times.Length)
            {
                throw new ArgumentException($"Values and times given to {nameof(AutomationEntity)} constructor must be the same length");
            }

            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, interpolationMethod, loop));
            Values keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<float, KeyframeTime>());
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (int i = 0; i < values.Length; i++)
            {
                keyframeValues.Write(i * keyframeSize, values[i]);
            }
        }

        public AutomationEntity(World world, ReadOnlySpan<float> times, ReadOnlySpan<T> values, bool loop = false)
        {
            if (values.Length != times.Length)
            {
                throw new ArgumentException($"Values and times given to {nameof(AutomationEntity)} constructor must be the same length");
            }

            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, default, loop));
            Values keyframeValues = world.CreateArray(entity, keyframeType, values.Length);
            world.CreateArray(entity, times.As<float, KeyframeTime>());
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (int i = 0; i < values.Length; i++)
            {
                keyframeValues.Write(i * keyframeSize, values[i]);
            }
        }

        public unsafe AutomationEntity(World world, InterpolationMethod interpolationMethod, ReadOnlySpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, interpolationMethod, loop));
            Values values = world.CreateArray(entity, keyframeType, keyframes.Length);
            Values<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (int i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Write(i * keyframeSize, keyframes[i].value);
            }
        }

        public unsafe AutomationEntity(World world, ReadOnlySpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, default, loop));
            Values values = world.CreateArray(entity, keyframeType, keyframes.Length);
            Values<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new Entity(world, entity).As<AutomationEntity>();

            ushort keyframeSize = AutomationEntity.GetKeyframeSize<T>();
            for (int i = 0; i < keyframes.Length; i++)
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
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(automation.world.Schema);
            ref IsAutomation component = ref automation.GetComponent<IsAutomation>();
            component.keyframeType = keyframeType;

            Values values = automation.GetArray(keyframeType.ArrayType);
            int keyframeCount = values.Length;
            values.Length++;
            Values times = automation.GetArray<KeyframeTime>();
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