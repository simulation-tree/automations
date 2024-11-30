using Automations.Components;
using System;
using System.Diagnostics;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly struct StateMachine : IEntity
    {
        private readonly Entity entity;

        public readonly USpan<AvailableState> AvailableStates => entity.GetArray<AvailableState>();
        public readonly USpan<Transition> Transitions => entity.GetArray<Transition>();

        public readonly FixedString EntryState
        {
            get
            {
                ThrowIfNoStatesAvailable();
                ThrowIfEntryStateIsUnassigned();
                ref IsStateMachine stateMachine = ref entity.GetComponentRef<IsStateMachine>();
                AvailableState state = AvailableStates[stateMachine.entryState - 1];
                return state.name;
            }
            set
            {
                ThrowIfStateIsMissing(value);
                ref IsStateMachine stateMachine = ref entity.GetComponentRef<IsStateMachine>();
                USpan<AvailableState> states = AvailableStates;
                for (uint index = 0; index < states.Length; index++)
                {
                    if (states[index].name == value)
                    {
                        stateMachine.entryState = index + 1;
                        return;
                    }
                }
            }
        }

        readonly uint IEntity.Value => entity.GetEntityValue();
        readonly World IEntity.World => entity.GetWorld();
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsStateMachine>().AddArrayTypes<AvailableState, Transition>();

#if NET
        [Obsolete("Default constructor not available", true)]
        public StateMachine()
        {
            throw new NotSupportedException();
        }
#endif
        /// <summary>
        /// Creates an empty uninitialized state machine
        /// with no states available.
        /// </summary>
        public StateMachine(World world)
        {
            entity = new(world);
            entity.AddComponent(new IsStateMachine(0));
            entity.CreateArray<AvailableState>(0);
            entity.CreateArray<Transition>(0);
        }

        /// <summary>
        /// Creates a new state machine initialized with
        /// the given states and transitions.
        /// <para>The first available will be set as the
        /// entry state.</para>
        /// </summary>
        public StateMachine(World world, USpan<AvailableState> states, USpan<Transition> transitions, uint entryState = 1)
        {
            entity = new(world);
            entity.AddComponent(new IsStateMachine(entryState));
            entity.CreateArray(states);
            entity.CreateArray(transitions);
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly void AddTransition(FixedString sourceState, FixedString destinationState, FixedString parameter, Transition.Condition condition, float value)
        {
            ThrowIfTransitionAlreadyExists(sourceState, destinationState, parameter);
            USpan<Transition> transitions = Transitions;
            uint transitionCount = transitions.Length;
            transitions = entity.ResizeArray<Transition>(transitionCount + 1);
            transitions[transitionCount] = new(sourceState, destinationState, parameter, condition, value);
        }

        public readonly bool ContainsTransition(FixedString sourceState, FixedString destinationState, FixedString parameter)
        {
            USpan<Transition> transitions = Transitions;
            int sourceStateHash = sourceState.GetHashCode();
            int destinationStateHash = destinationState.GetHashCode();
            int parameterHash = parameter.GetHashCode();
            for (uint i = 0; i < transitions.Length; i++)
            {
                ref Transition transition = ref transitions[i];
                if (transition.sourceStateHash == sourceStateHash && transition.destinationStateHash == destinationStateHash && transition.parameterHash == parameterHash)
                {
                    return true;
                }
            }

            return false;
        }

        public readonly ref Transition GetTransition(FixedString sourceState, FixedString destinationState, FixedString parameter)
        {
            USpan<Transition> transitions = Transitions;
            int sourceStateHash = sourceState.GetHashCode();
            int destinationStateHash = destinationState.GetHashCode();
            int parameterHash = parameter.GetHashCode();
            for (uint i = 0; i < transitions.Length; i++)
            {
                ref Transition transition = ref transitions[i];
                if (transition.sourceStateHash == sourceStateHash && transition.destinationStateHash == destinationStateHash && transition.parameterHash == parameterHash)
                {
                    return ref transition;
                }
            }

            throw new NullReferenceException($"Transition from `{sourceState}` to `{destinationState}` with parameter `{parameter}` not found");
        }

        public readonly void AddState(FixedString name)
        {
            ThrowIfAvailableStateAlreadyExists(name);
            uint availableStateCount = AvailableStates.Length;
            USpan<AvailableState> availableStates = entity.ResizeArray<AvailableState>(availableStateCount + 1);
            availableStates[availableStateCount] = new(name);
        }

        public readonly bool ContainsState(FixedString name)
        {
            USpan<AvailableState> availableStates = AvailableStates;
            for (uint i = 0; i < availableStates.Length; i++)
            {
                if (availableStates[i].name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public readonly ref AvailableState GetState(FixedString name)
        {
            USpan<AvailableState> availableStates = AvailableStates;
            for (uint i = 0; i < availableStates.Length; i++)
            {
                if (availableStates[i].name == name)
                {
                    return ref availableStates[i];
                }
            }

            throw new NullReferenceException($"Available state `{name}` not found");
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfTransitionAlreadyExists(FixedString sourceState, FixedString destinationState, FixedString parameter)
        {
            if (ContainsTransition(sourceState, destinationState, parameter))
            {
                throw new InvalidOperationException($"Transition from `{sourceState}` to `{destinationState}` with parameter `{parameter}` already exists");
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfAvailableStateAlreadyExists(FixedString name)
        {
            if (ContainsState(name))
            {
                throw new InvalidOperationException($"Available state with name `{name}` already exists");
            }
        }

        [Conditional("DEBUG")]
        public readonly void ThrowIfStateIsMissing(FixedString name)
        {
            if (!ContainsState(name))
            {
                throw new InvalidOperationException($"State `{name}` is missing");
            }
        }

        [Conditional("DEBUG")]
        public readonly void ThrowIfNoStatesAvailable()
        {
            if (AvailableStates.Length == 0)
            {
                throw new InvalidOperationException($"No states available on state machine `{entity}`");
            }
        }

        [Conditional("DEBUG")]
        public readonly void ThrowIfEntryStateIsUnassigned()
        {
            ref IsStateMachine stateMachine = ref entity.GetComponentRef<IsStateMachine>();
            if (stateMachine.entryState == default)
            {
                throw new InvalidOperationException($"State machine `{entity}` has no entry state unassigned");
            }
        }

        public static implicit operator Entity(StateMachine stateMachine)
        {
            return stateMachine.entity;
        }
    }
}