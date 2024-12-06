﻿using Automations.Components;
using Collections;
using Simulation;
using System;
using Unmanaged;
using Worlds;

namespace Automations.Systems
{
    public readonly partial struct AutomationPlayingSystem : ISystem
    {
        private readonly List<Interpolation> interpolationFunctions;

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            ComponentQuery<IsAutomationPlayer> query = new(world);
            foreach (var r in query)
            {
                uint entity = r.entity;
                ref IsAutomationPlayer player = ref r.component1;
                if (player.automationReference != default)
                {
                    player.time += delta;
                    uint automationEntity = world.GetReference(entity, player.automationReference);
                    Evaluate(world, entity, player.componentType, automationEntity, player.time);
                }
            }
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                interpolationFunctions.Dispose();
            }
        }

        public AutomationPlayingSystem()
        {
            interpolationFunctions = new();
            foreach (Interpolation interpolation in BuiltInInterpolations.all)
            {
                AddInterpolation(interpolation);
            }
        }

        public readonly InterpolationMethod AddInterpolation(Interpolation interpolation)
        {
            interpolationFunctions.Add(interpolation);
            return new((byte)interpolationFunctions.Count);
        }

        private readonly unsafe void Evaluate(World world, uint playerEntity, ComponentType componentType, uint automationEntity, TimeSpan time)
        {
            IsAutomation automationComponent = world.GetComponent<IsAutomation>(automationEntity);
            ArrayType keyframeType = automationComponent.keyframeType;
            Allocation keyframeValues = world.GetArray(automationEntity, keyframeType, out uint keyframeCount);
            USpan<float> keyframeTimes = world.GetArray<KeyframeTime>(automationEntity).As<float>();
            if (keyframeCount == 0)
            {
                return;
            }

            float timeInSeconds = (float)time.TotalSeconds;
            uint keyframeTypeSize = keyframeType.Size;
            float finalKeyframeTime = keyframeTimes[keyframeCount - 1];
            if (timeInSeconds >= finalKeyframeTime)
            {
                if (automationComponent.loop)
                {
                    timeInSeconds %= finalKeyframeTime;
                }
                else
                {
                    timeInSeconds = finalKeyframeTime;
                }
            }

            uint current = 0;
            if (keyframeCount > 0)
            {
                for (uint i = 0; i < keyframeCount; i++)
                {
                    float keyframeTime = keyframeTimes[i];
                    if (timeInSeconds >= keyframeTime)
                    {
                        current = i;
                    }
                }
            }

            bool loop = automationComponent.loop;
            uint next = current + 1;
            if (next == keyframeCount)
            {
                if (loop)
                {
                    next = 0;
                }
                else
                {
                    next = current;
                }
            }

            void* currentKeyframe = keyframeValues.Read(current * keyframeTypeSize);
            void* nextKeyframe = keyframeValues.Read(next * keyframeTypeSize);
            float currentKeyframeTime = keyframeTimes[current];
            float nextKeyframeTime = keyframeTimes[next];
            float timeDelta = nextKeyframeTime - currentKeyframeTime;
            float timeProgress = (timeInSeconds - currentKeyframeTime) / timeDelta;
            if (float.IsNaN(timeProgress))
            {
                timeProgress = 0f;
            }

            if (automationComponent.interpolationMethod == default)
            {
                void* component = world.GetComponent(playerEntity, componentType);
                System.Runtime.CompilerServices.Unsafe.CopyBlock(component, currentKeyframe, componentType.Size);
            }
            else
            {
                Interpolation interpolation = interpolationFunctions[(byte)(automationComponent.interpolationMethod.value - 1)];
                void* component = world.GetComponent(playerEntity, componentType);
                interpolation.Invoke(currentKeyframe, nextKeyframe, timeProgress, component, componentType.Size);
            }
        }
    }
}