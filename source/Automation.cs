using Automations.Components;
using Simulation;
using System;
using Unmanaged;

namespace Automations
{
    public readonly struct Automation : IEntity
    {
        public readonly Entity entity;

        public readonly ref bool Loop => ref entity.GetComponentRef<IsAutomation>().loop;

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsAutomation>()], []);

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
    }

    public readonly struct Automation<T> : IEntity where T : unmanaged
    {
        public readonly Automation automation;

        public readonly uint KeyframeCount => automation.entity.GetArrayLength<Keyframe<T>>();
        public readonly ref Keyframe<T> this[uint index] => ref automation.entity.GetArrayElementRef<Keyframe<T>>(index);
        public readonly USpan<Keyframe<T>> Keyframes => automation.entity.GetArray<Keyframe<T>>();
        public readonly ref bool Loop => ref automation.Loop;

        readonly uint IEntity.Value => automation.entity.value;
        readonly World IEntity.World => automation.entity.world;
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsAutomation>()], [RuntimeType.Get<Keyframe<T>>()]);

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
        public Automation(World world, bool loop = false)
        {
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(RuntimeType.Get<Keyframe<T>>(), loop));
            world.CreateArray<Keyframe<T>>(entity);
            automation = new(world, entity);
        }

        public Automation(World world, USpan<Keyframe<T>> keyframes, bool loop = false)
        {
            uint entity = world.CreateEntity();
            world.AddComponent(entity, new IsAutomation(RuntimeType.Get<Keyframe<T>>(), loop));
            world.CreateArray(entity, keyframes);
            automation = new(world, entity);
        }

        public readonly void AddKeyframe(float time, T value)
        {
            uint keyframeCount = KeyframeCount;
            USpan<Keyframe<T>> keyframes = automation.entity.ResizeArray<Keyframe<T>>(keyframeCount + 1);
            ref Keyframe<T> keyframe = ref keyframes[keyframeCount];
            keyframe.time = time;
            keyframe.value = value;
        }

        public static implicit operator Automation(Automation<T> automation)
        {
            return automation.automation;
        }
    }
}