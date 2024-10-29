using Automations.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Runtime.InteropServices;
using Unmanaged;

namespace Automations.Systems
{
    public readonly struct StateAutomationSystem : ISystem
    {
        private readonly ComponentQuery<IsStateful, IsAutomationPlayer> statefulQuery;

        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Update => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref StateAutomationSystem system = ref container.Read<StateAutomationSystem>();
            system.Update(world);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref StateAutomationSystem system = ref container.Read<StateAutomationSystem>();
                system.CleanUp();
            }
        }

        public StateAutomationSystem()
        {
            statefulQuery = new();
        }

        private void CleanUp()
        {
            statefulQuery.Dispose();
        }

        private void Update(World world)
        {
            statefulQuery.Update(world);
            foreach (var x in statefulQuery)
            {
                uint statefulEntity = x.entity;
                IsStateful statefulComponent = x.Component1;
                if (statefulComponent.state == default)
                {
                    //state not yet assigned
                    continue;
                }

                rint stateMachineReference = statefulComponent.stateMachineReference;
                uint stateMachineEntity = world.GetReference(statefulEntity, stateMachineReference);
                USpan<AvailableState> states = world.GetArray<AvailableState>(stateMachineEntity);
                AvailableState state = states[statefulComponent.state - 1];
                int stateNameHash = state.name.GetHashCode();
                USpan<StateAutomationLink> links = world.GetArray<StateAutomationLink>(statefulEntity);
                foreach (StateAutomationLink link in links)
                {
                    if (link.stateNameHash == stateNameHash)
                    {
                        ref IsAutomationPlayer player = ref x.Component2;
                        ref rint automationReference = ref player.automationReference;
                        uint desiredAutomationEntity = world.GetReference(statefulEntity, link.automationReference);
                        if (automationReference == default)
                        {
                            player.time = default;
                            player.componentType = link.componentType;
                            automationReference = world.AddReference(statefulEntity, desiredAutomationEntity);
                        }
                        else
                        {
                            uint currentAutomationEntity = world.GetReference(statefulEntity, automationReference);
                            if (currentAutomationEntity != desiredAutomationEntity)
                            {
                                player.time = default;
                                player.componentType = link.componentType;
                                world.SetReference(statefulEntity, automationReference, desiredAutomationEntity);
                            }
                            else
                            {
                                //automation already set
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}