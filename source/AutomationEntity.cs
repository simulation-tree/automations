using Automations.Components;
using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly partial struct AutomationEntity : IEntity
    {
        public readonly ref bool Loop => ref GetComponent<IsAutomation>().loop;
        public readonly ReadOnlySpan<float> KeyframeTimes => GetArray<KeyframeTime>().AsSpan<float>();
        public readonly Values KeyframeValues => GetArray(KeyframeType.index);
        public readonly DataType KeyframeType => GetComponent<IsAutomation>().keyframeType;
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
            int containingSize = (sizeof(T) - 1).GetNextPowerOf2();
            int index = containingSize.GetIndexOfPowerOf2();
            return index switch
            {
                0 => new(schema.GetArrayType<KeyframeValue1>(), DataType.Kind.Array, 1),
                1 => new(schema.GetArrayType<KeyframeValue2>(), DataType.Kind.Array, 2),
                2 => new(schema.GetArrayType<KeyframeValue4>(), DataType.Kind.Array, 4),
                3 => new(schema.GetArrayType<KeyframeValue8>(), DataType.Kind.Array, 8),
                4 => new(schema.GetArrayType<KeyframeValue16>(), DataType.Kind.Array, 16),
                5 => new(schema.GetArrayType<KeyframeValue32>(), DataType.Kind.Array, 32),
                6 => new(schema.GetArrayType<KeyframeValue64>(), DataType.Kind.Array, 64),
                7 => new(schema.GetArrayType<KeyframeValue128>(), DataType.Kind.Array, 128),
                8 => new(schema.GetArrayType<KeyframeValue256>(), DataType.Kind.Array, 256),
                _ => throw new NotSupportedException($"Keyframe value type `{typeof(T)}` is greater than the maximum 256")
            };
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
                DataType keyframeType = AutomationEntity.GetKeyframeType<T>(schema);
                ref T value = ref automation.GetArray(keyframeType.index).Get<T>(index);
                ref float time = ref automation.GetArrayElement<KeyframeTime>(index).time;
                return (time, value);
            }
            set
            {
                Schema schema = automation.world.Schema;
                DataType keyframeType = AutomationEntity.GetKeyframeType<T>(schema);
                ref T keyframeValue = ref automation.GetArray(keyframeType.index).Get<T>(index);
                ref float keyframeTime = ref automation.GetArrayElement<KeyframeTime>(index).time;
                keyframeValue = value.value;
                keyframeTime = value.time;
            }
        }

        public readonly ref bool Loop => ref automation.Loop;
        public readonly DataType KeyframeType => automation.KeyframeType;

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsAutomation>();
            archetype.AddArrayType(AutomationEntity.GetKeyframeType<T>(archetype.schema).index);
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

            for (int i = 0; i < values.Length; i++)
            {
                keyframeValues.Set(i, values[i]);
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

            for (int i = 0; i < values.Length; i++)
            {
                keyframeValues.Set(i, values[i]);
            }
        }

        public unsafe AutomationEntity(World world, InterpolationMethod interpolationMethod, ReadOnlySpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, interpolationMethod, loop));
            Values values = world.CreateArray(entity, keyframeType, keyframes.Length);
            Values<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new Entity(world, entity).As<AutomationEntity>();

            for (int i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Set(i, keyframes[i].value);
            }
        }

        public unsafe AutomationEntity(World world, ReadOnlySpan<(float time, T value)> keyframes, bool loop = false)
        {
            DataType keyframeType = AutomationEntity.GetKeyframeType<T>(world.Schema);
            uint entity = world.CreateEntity(new IsAutomation(keyframeType, default, loop));
            Values values = world.CreateArray(entity, keyframeType, keyframes.Length);
            Values<KeyframeTime> times = world.CreateArray<KeyframeTime>(entity, keyframes.Length);
            automation = new Entity(world, entity).As<AutomationEntity>();

            for (int i = 0; i < keyframes.Length; i++)
            {
                times[i] = keyframes[i].time;
                values.Set(i, keyframes[i].value);
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

            Values values = automation.GetArray(keyframeType.index);
            Values times = automation.GetArray<KeyframeTime>();
            values.Add(value);
            times.Add(time);
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