using Automations.Components;
using Automations.Events;
using Simulation;
using Unmanaged;

namespace Automations.Systems
{
    public class StateAutomationSystem : SystemBase
    {
        private readonly ComponentQuery<IsStateful, IsAutomationPlayer> statefulQuery;

        public StateAutomationSystem(World world) : base(world)
        {
            statefulQuery = new();
            Subscribe<StateUpdate>(Update);
        }

        public override void Dispose()
        {
            statefulQuery.Dispose();
            base.Dispose();
        }

        private void Update(StateUpdate message)
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