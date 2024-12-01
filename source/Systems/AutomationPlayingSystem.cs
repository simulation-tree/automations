using Automations.Components;
using Collections;
using Simulation;
using Simulation.Functions;
using System;
using System.Runtime.InteropServices;
using Unmanaged;
using Worlds;

namespace Automations.Systems
{
    public readonly struct AutomationPlayingSystem : ISystem
    {
        private readonly ComponentQuery<IsAutomationPlayer> playerQuery;
        private readonly List<Interpolation> interpolationFunctions;

        readonly unsafe StartSystem ISystem.Start => new(&Start);
        readonly unsafe UpdateSystem ISystem.Update => new(&Update);
        readonly unsafe FinishSystem ISystem.Finish => new(&Finish);

        [UnmanagedCallersOnly]
        private static void Start(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref AutomationPlayingSystem system = ref container.Read<AutomationPlayingSystem>();
            system.Update(world, delta);
        }

        [UnmanagedCallersOnly]
        private static void Finish(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref AutomationPlayingSystem system = ref container.Read<AutomationPlayingSystem>();
                system.CleanUp();
            }
        }

        public AutomationPlayingSystem()
        {
            playerQuery = new();
            interpolationFunctions = new();
            foreach (Interpolation interpolation in BuiltInInterpolations.all)
            {
                AddInterpolation(interpolation);
            }
        }

        private readonly void CleanUp()
        {
            interpolationFunctions.Dispose();
            playerQuery.Dispose();
        }

        private readonly void Update(World world, TimeSpan delta)
        {
            playerQuery.Update(world);
            foreach (var x in playerQuery)
            {
                uint playerEntity = x.entity;
                ref IsAutomationPlayer player = ref x.Component1;
                if (player.automationReference != default)
                {
                    player.time += delta;
                    uint automationEntity = world.GetReference(playerEntity, player.automationReference);
                    Evaluate(world, playerEntity, player.componentType, automationEntity, player.time);
                }
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