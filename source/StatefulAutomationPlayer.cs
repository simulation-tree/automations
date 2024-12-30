using Automations.Components;
using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly struct StatefulAutomationPlayer : IEntity
    {
        private readonly Entity entity;

        public readonly StateMachine StateMachine
        {
            get
            {
                Stateful stateful = entity.As<Stateful>();
                return stateful.StateMachine;
            }
            set
            {
                Stateful stateful = entity.As<Stateful>();
                stateful.StateMachine = value;
            }
        }

        public readonly FixedString CurrentState
        {
            get
            {
                Stateful stateful = entity.As<Stateful>();
                return stateful.CurrentState;
            }
        }

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;

        readonly Definition IEntity.GetDefinition(Schema schema)
        {
            return new Definition().AddComponentTypes<IsStateful, IsAutomationPlayer>(schema).AddArrayElementTypes<Parameter, StateAutomationLink>(schema);
        }

#if NET
        [Obsolete("Default constructor not available", true)]
        public StatefulAutomationPlayer()
        {
            throw new NotSupportedException();
        }
#endif

        /// <summary>
        /// Creates a new stateful entity initialized to the
        /// entry state of the assigned state machine.
        /// </summary>
        public StatefulAutomationPlayer(World world, StateMachine stateMachine)
        {
            entity = new(world);
            uint state = stateMachine.AsEntity().GetComponent<IsStateMachine>().entryState;
            rint stateMachineReference = entity.AddReference(stateMachine);
            entity.AddComponent(new IsStateful(state, stateMachineReference));
            entity.AddComponent<IsAutomationPlayer>();
            entity.CreateArray<Parameter>();
            entity.CreateArray<StateAutomationLink>();
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly ref float AddParameter(FixedString name, float defaultValue = 0f)
        {
            Stateful stateful = entity.As<Stateful>();
            return ref stateful.AddParameter(name, defaultValue);
        }

        public readonly ref float GetParameterRef(FixedString name)
        {
            Stateful stateful = entity.As<Stateful>();
            return ref stateful.GetParameterRef(name);
        }

        public readonly bool ContainsParameter(FixedString name)
        {
            Stateful stateful = entity.As<Stateful>();
            return stateful.ContainsParameter(name);
        }

        public readonly void SetParameter(FixedString name, float value)
        {
            Stateful stateful = entity.As<Stateful>();
            stateful.SetParameter(name, value);
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update component <typeparamref name="T"/>.
        /// </summary>
        public readonly void AddOrSetLink<T>(FixedString stateName, Automation automation) where T : unmanaged
        {
            StateMachine.ThrowIfStateIsMissing(stateName);
            int stateNameHash = stateName.GetHashCode();
            USpan<StateAutomationLink> links = entity.GetArray<StateAutomationLink>();
            ComponentType componentType = entity.GetWorld().Schema.GetComponent<T>();
            uint count = links.Length;
            for (uint i = 0; i < count; i++)
            {
                ref StateAutomationLink existingLink = ref links[i];
                if (existingLink.stateNameHash == stateNameHash)
                {
                    rint automationReference = existingLink.automationReference;
                    uint automationEntity = entity.GetReference(automationReference);
                    existingLink.componentType = componentType;
                    if (automation.GetEntityValue() != automationEntity)
                    {
                        entity.SetReference(automationReference, automation);
                    }

                    return;
                }
            }

            links = entity.ResizeArray<StateAutomationLink>(count + 1);
            ref StateAutomationLink newLink = ref links[count];
            newLink.stateNameHash = stateNameHash;
            newLink.componentType = componentType;
            newLink.automationReference = entity.AddReference(automation);
        }

        public static implicit operator Entity(StatefulAutomationPlayer entity)
        {
            return entity.entity;
        }

        public static implicit operator Stateful(StatefulAutomationPlayer entity)
        {
            return entity.entity.As<Stateful>();
        }
    }
}