using Automations.Components;
using System;
using System.Diagnostics;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly struct Stateful : IEntity
    {
        private readonly Entity entity;

        public readonly USpan<Parameter> Parameters => entity.GetArray<Parameter>();

        public readonly StateMachine StateMachine
        {
            get
            {
                rint stateMachineReference = entity.GetComponent<IsStateful>().stateMachineReference;
                uint stateMachineEntity = entity.GetReference(stateMachineReference);
                return new Entity(entity.GetWorld(), stateMachineEntity).As<StateMachine>();
            }
            set
            {
                ref IsStateful component = ref entity.GetComponent<IsStateful>();
                if (component.stateMachineReference == default)
                {
                    component.stateMachineReference = entity.AddReference(value);
                    component.state = value.AsEntity().GetComponent<IsStateMachine>().entryState;
                }
                else
                {
                    uint stateMachineEntity = entity.GetReference(component.stateMachineReference);
                    if (stateMachineEntity != value.GetEntityValue())
                    {
                        entity.SetReference(component.stateMachineReference, value);
                        component.state = value.AsEntity().GetComponent<IsStateMachine>().entryState;
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
                ref IsStateful isStateful = ref entity.GetComponent<IsStateful>();
                AvailableState state = StateMachine.AvailableStates[isStateful.state - 1];
                return state.name;
            }
        }

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsStateful>();
            archetype.AddArrayElementType<Parameter>();
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public Stateful()
        {
            throw new NotSupportedException();
        }
#endif

        public Stateful(World world, uint existingEntity)
        {
            entity = new(world, existingEntity);
        }

        /// <summary>
        /// Creates a new stateful entity initialized to the
        /// entry state of the assigned state machine.
        /// </summary>
        public Stateful(World world, StateMachine stateMachine)
        {
            entity = new(world);
            entity.CreateArray<Parameter>(0);
            uint state = stateMachine.AsEntity().GetComponent<IsStateMachine>().entryState;
            rint stateMachineReference = entity.AddReference(stateMachine);
            entity.AddComponent(new IsStateful(state, stateMachineReference));
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly ref float AddParameter(FixedString name, float defaultValue = 0f)
        {
            ThrowIfParameterAlreadyExists(name);
            uint parameterCount = entity.GetArrayLength<Parameter>();
            USpan<Parameter> parameters = entity.ResizeArray<Parameter>(parameterCount + 1);
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

            parameters = entity.ResizeArray<Parameter>(count + 1);
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
            ref IsStateful isStateful = ref entity.GetComponent<IsStateful>();
            if (isStateful.state == default)
            {
                throw new InvalidOperationException($"Stateful entity `{entity}` has no assigned state");
            }
        }

        public static implicit operator Entity(Stateful stateful)
        {
            return stateful.entity;
        }
    }
}