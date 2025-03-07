using Automations.Components;
using System;
using System.Diagnostics;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly partial struct StateMachine : IEntity
    {
        public readonly USpan<AvailableState> AvailableStates => GetArray<AvailableState>().AsSpan();
        public readonly USpan<Transition> Transitions => GetArray<Transition>().AsSpan();
        public readonly uint EntryStateIndex => GetComponent<IsStateMachine>().entryState;

        public readonly ASCIIText256 EntryState
        {
            get
            {
                ThrowIfNoStatesAvailable();
                ThrowIfEntryStateIsUnassigned();

                ref IsStateMachine stateMachine = ref GetComponent<IsStateMachine>();
                AvailableState state = AvailableStates[stateMachine.entryState - 1];
                return state.name;
            }
            set
            {
                ThrowIfStateIsMissing(value);

                ref IsStateMachine stateMachine = ref GetComponent<IsStateMachine>();
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

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsStateMachine>();
            archetype.AddArrayType<AvailableState>();
            archetype.AddArrayType<Transition>();
        }

        /// <summary>
        /// Creates an empty uninitialized state machine
        /// with no states available.
        /// </summary>
        public StateMachine(World world)
        {
            this.world = world;
            value = world.CreateEntity(new IsStateMachine(0));
            CreateArray<AvailableState>(0);
            CreateArray<Transition>(0);
        }

        /// <summary>
        /// Creates a new state machine initialized with
        /// the given states and transitions.
        /// <para>The first available will be set as the
        /// entry state.</para>
        /// </summary>
        public StateMachine(World world, USpan<AvailableState> states, USpan<Transition> transitions, uint entryState = 1)
        {
            this.world = world;
            value = world.CreateEntity(new IsStateMachine(entryState));
            CreateArray(states);
            CreateArray(transitions);
        }

        public readonly void AddTransition(ASCIIText256 sourceState, ASCIIText256 destinationState, ASCIIText256 parameter, Transition.Condition condition, float value)
        {
            ThrowIfTransitionAlreadyExists(sourceState, destinationState, parameter);

            Values<Transition> transitions = GetArray<Transition>();
            uint transitionCount = transitions.Length;
            transitions.Length++;
            transitions[transitionCount] = new(sourceState, destinationState, parameter, condition, value);
        }

        public readonly bool ContainsTransition(ASCIIText256 sourceState, ASCIIText256 destinationState, ASCIIText256 parameter)
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

        public readonly ref Transition GetTransition(ASCIIText256 sourceState, ASCIIText256 destinationState, ASCIIText256 parameter)
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

        public readonly void AddState(ASCIIText256 name)
        {
            ThrowIfAvailableStateAlreadyExists(name);

            Values<AvailableState> availableStates = GetArray<AvailableState>();
            uint availableStateCount = availableStates.Length;
            availableStates.Length++;
            availableStates[availableStateCount] = new(name);
        }

        public readonly bool ContainsState(ASCIIText256 name)
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

        public readonly ref AvailableState GetState(ASCIIText256 name)
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
        private readonly void ThrowIfTransitionAlreadyExists(ASCIIText256 sourceState, ASCIIText256 destinationState, ASCIIText256 parameter)
        {
            if (ContainsTransition(sourceState, destinationState, parameter))
            {
                throw new InvalidOperationException($"Transition from `{sourceState}` to `{destinationState}` with parameter `{parameter}` already exists");
            }
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfAvailableStateAlreadyExists(ASCIIText256 name)
        {
            if (ContainsState(name))
            {
                throw new InvalidOperationException($"Available state with name `{name}` already exists");
            }
        }

        [Conditional("DEBUG")]
        public readonly void ThrowIfStateIsMissing(ASCIIText256 name)
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
                throw new InvalidOperationException($"No states available on state machine `{value}`");
            }
        }

        [Conditional("DEBUG")]
        public readonly void ThrowIfEntryStateIsUnassigned()
        {
            ref IsStateMachine stateMachine = ref GetComponent<IsStateMachine>();
            if (stateMachine.entryState == default)
            {
                throw new InvalidOperationException($"State machine `{value}` has no entry state unassigned");
            }
        }
    }
}