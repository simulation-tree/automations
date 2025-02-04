using Automations.Components;
using System;
using System.Diagnostics;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly partial struct Stateful : IEntity
    {
        public readonly USpan<Parameter> Parameters => GetArray<Parameter>();

        public readonly StateMachine StateMachine
        {
            get
            {
                rint stateMachineReference = GetComponent<IsStateful>().stateMachineReference;
                uint stateMachineEntity = GetReference(stateMachineReference);
                return new Entity(world, stateMachineEntity).As<StateMachine>();
            }
            set
            {
                ref IsStateful component = ref GetComponent<IsStateful>();
                if (component.stateMachineReference == default)
                {
                    component.stateMachineReference = AddReference(value);
                    component.state = value.GetComponent<IsStateMachine>().entryState;
                }
                else
                {
                    uint stateMachineEntity = GetReference(component.stateMachineReference);
                    if (stateMachineEntity != value.value)
                    {
                        SetReference(component.stateMachineReference, value);
                        component.state = value.GetComponent<IsStateMachine>().entryState;
                    }
                    else
                    {
                        //same state machine
                    }
                }
            }
        }

        public readonly FixedString CurrentState
        {
            get
            {
                ThrowIfStateIsUnassigned();

                ref IsStateful isStateful = ref GetComponent<IsStateful>();
                AvailableState state = StateMachine.AvailableStates[isStateful.state - 1];
                return state.name;
            }
        }

        /// <summary>
        /// Creates a new stateful entity initialized to the
        /// entry state of the assigned state machine.
        /// </summary>
        public Stateful(World world, StateMachine stateMachine)
        {
            this.world = world;
            value = world.CreateEntity(new IsStateful(stateMachine.EntryStateIndex, (rint)1));
            CreateArray<Parameter>(0);
            AddReference(stateMachine);
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsStateful>();
            archetype.AddArrayType<Parameter>();
        }

        public readonly ref float AddParameter(FixedString name, float defaultValue = 0f)
        {
            ThrowIfParameterAlreadyExists(name);

            uint parameterCount = GetArrayLength<Parameter>();
            USpan<Parameter> parameters = ResizeArray<Parameter>(parameterCount + 1);
            ref Parameter newParameter = ref parameters[parameterCount];
            newParameter.name = name;
            newParameter.value = defaultValue;
            return ref newParameter.value;
        }

        public readonly bool ContainsParameter(FixedString name)
        {
            USpan<Parameter> parameters = Parameters;
            for (uint i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public readonly ref float GetParameterRef(FixedString name)
        {
            USpan<Parameter> parameters = Parameters;
            for (uint i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == name)
                {
                    return ref parameters[i].value;
                }
            }

            throw new NullReferenceException($"Parameter `{name}` not found");
        }

        public readonly void SetParameter(FixedString name, float value)
        {
            ref float parameter = ref GetParameterRef(name);
            parameter = value;
        }

        public readonly void AddOrSetParameter(FixedString name, float value)
        {
            USpan<Parameter> parameters = Parameters;
            uint count = parameters.Length;
            for (uint i = 0; i < count; i++)
            {
                if (parameters[i].name == name)
                {
                    parameters[i].value = value;
                    return;
                }
            }

            parameters = ResizeArray<Parameter>(count + 1);
            ref Parameter newParameter = ref parameters[count];
            newParameter.name = name;
            newParameter.value = value;
        }

        [Conditional("DEBUG")]
        private readonly void ThrowIfParameterAlreadyExists(FixedString name)
        {
            if (ContainsParameter(name))
            {
                throw new InvalidOperationException($"Parameter `{name}` already exists");
            }
        }

        [Conditional("DEBUG")]
        public readonly void ThrowIfStateIsUnassigned()
        {
            ref IsStateful isStateful = ref GetComponent<IsStateful>();
            if (isStateful.state == default)
            {
                throw new InvalidOperationException($"Stateful entity `{value}` has no assigned state");
            }
        }
    }
}