using Automations.Components;
using Automations.Events;
using Simulation;
using System;
using Unmanaged;

namespace Automations.Systems
{
    public class StateMachineSystem : SystemBase
    {
        private readonly ComponentQuery<IsStateful> statefulQuery;

        public StateMachineSystem(World world) : base(world)
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
                ref IsStateful stateful = ref x.Component1;
                if (stateful.stateMachineReference == default)
                {
                    throw new InvalidOperationException($"Stateful entity `{statefulEntity}` does not have a state machine reference");
                }

                USpan<Parameter> parameters = world.GetArray<Parameter>(statefulEntity);
                uint stateMachineEntity = world.GetReference(statefulEntity, stateful.stateMachineReference);
                USpan<AvailableState> availableStates = world.GetArray<AvailableState>(stateMachineEntity);
                if (stateful.state == default)
                {
                    stateful.state = world.GetComponent<IsStateMachine>(stateMachineEntity).entryState;
                    if (stateful.state == default)
                    {
                        throw new InvalidOperationException($"State machine `{stateMachineEntity}` does not have an entry state assigned");
                    }
                }

                AvailableState currentState = availableStates[stateful.state - 1];
                int currentStateHash = currentState.name.GetHashCode();
                USpan<Transition> transitions = world.GetArray<Transition>(stateMachineEntity);
                foreach (Transition transition in transitions)
                {
                    if (transition.sourceStateHash == currentStateHash)
                    {
                        if (IsConditionMet(transition, parameters))
                        {
                            if (TryGetAvailableStateIndex(transition.destinationStateHash, availableStates, out uint newStateIndex))
                            {
                                stateful.state = newStateIndex + 1;
                                break;
                            }
                            else
                            {
                                throw new InvalidOperationException($"State with name hash `{transition.destinationStateHash}` on state machine `{stateMachineEntity}` couldn't be found");
                            }
                        }
                    }
                }
            }
        }

        private static bool IsConditionMet(Transition transition, USpan<Parameter> parameters)
        {
            float value = transition.value;
            Transition.Condition condition = transition.condition;
            float parameterValue = GetParameterValue(transition.parameterHash, parameters);
            if (condition == Transition.Condition.Equal)
            {
                return parameterValue == value;
            }
            else if (condition == Transition.Condition.NotEqual)
            {
                return parameterValue != value;
            }
            else if (condition == Transition.Condition.GreaterThan)
            {
                return parameterValue > value;
            }
            else if (condition == Transition.Condition.GreaterThanOrEqual)
            {
                return parameterValue >= value;
            }
            else if (condition == Transition.Condition.LessThan)
            {
                return parameterValue < value;
            }
            else if (condition == Transition.Condition.LessThanOrEqual)
            {
                return parameterValue <= value;
            }
            else if (condition == Transition.Condition.None)
            {
                return false;
            }
            else
            {
                throw new NotSupportedException($"Unsupported condition `{condition}`");
            }
        }

        private static float GetParameterValue(int nameHash, USpan<Parameter> parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter.name.GetHashCode() == nameHash)
                {
                    return parameter.value;
                }
            }

            throw new InvalidOperationException($"Parameter with name hash `{nameHash}` not found");
        }

        private static bool TryGetAvailableStateIndex(int nameHash, USpan<AvailableState> availableStates, out uint index)
        {
            for (uint i = 0; i < availableStates.Length; i++)
            {
                if (availableStates[i].name.GetHashCode() == nameHash)
                {
                    index = i;
                    return true;
                }
            }

            index = default;
            return false;
        }
    }
}