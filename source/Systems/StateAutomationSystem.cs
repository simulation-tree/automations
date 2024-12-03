using Automations.Components;
using Simulation;
using System;
using Unmanaged;
using Worlds;

namespace Automations.Systems
{
    public readonly partial struct StateAutomationSystem : ISystem
    {
        private readonly ComponentQuery<IsStateful, IsAutomationPlayer> statefulQuery;

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            Update(world);
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                CleanUp();
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