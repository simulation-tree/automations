using Automations.Components;
using System;
using Types;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly partial struct StatefulAutomationPlayer : IEntity
    {
        public readonly StateMachine StateMachine
        {
            get
            {
                Stateful stateful = As<Stateful>();
                return stateful.StateMachine;
            }
            set
            {
                Stateful stateful = As<Stateful>();
                stateful.StateMachine = value;
            }
        }

        public readonly ASCIIText256 CurrentState
        {
            get
            {
                Stateful stateful = As<Stateful>();
                return stateful.CurrentState;
            }
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.Add<Stateful>();
            archetype.Add<AutomationPlayer>();
            archetype.AddArrayType<StateAutomationLink>();
        }

        /// <summary>
        /// Creates a new stateful entity initialized to the
        /// entry state of the assigned state machine.
        /// </summary>
        public StatefulAutomationPlayer(World world, StateMachine stateMachine)
        {
            this.world = world;
            value = world.CreateEntity(new IsStateful(stateMachine.EntryStateIndex, (rint)1), new IsAutomationPlayer(default, default));
            AddReference(stateMachine);
            CreateArray<Parameter>();
            CreateArray<StateAutomationLink>();
        }

        public readonly ref float AddParameter(ASCIIText256 name, float defaultValue = 0f)
        {
            Stateful stateful = As<Stateful>();
            return ref stateful.AddParameter(name, defaultValue);
        }

        public readonly ref float GetParameterRef(ASCIIText256 name)
        {
            Stateful stateful = As<Stateful>();
            return ref stateful.GetParameterRef(name);
        }

        public readonly bool ContainsParameter(ASCIIText256 name)
        {
            Stateful stateful = As<Stateful>();
            return stateful.ContainsParameter(name);
        }

        public readonly void SetParameter(ASCIIText256 name, float value)
        {
            Stateful stateful = As<Stateful>();
            stateful.SetParameter(name, value);
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update component <typeparamref name="T"/>.
        /// </summary>
        public readonly void AddOrSetLinkToComponent<T>(ASCIIText256 stateName, AutomationEntity automation, int byteOffset = 0) where T : unmanaged
        {
            StateMachine.ThrowIfStateIsMissing(stateName);

            int stateNameHash = stateName.GetHashCode();
            Values<StateAutomationLink> links = GetArray<StateAutomationLink>();
            DataType targetType = world.Schema.GetComponentDataType<T>();
            int count = links.Length;
            for (int i = 0; i < count; i++)
            {
                ref StateAutomationLink existingLink = ref links[i];
                if (existingLink.stateNameHash == stateNameHash)
                {
                    rint automationReference = existingLink.automationReference;
                    uint automationEntity = GetReference(automationReference);
                    existingLink.target = new(targetType, byteOffset);
                    if (automation.GetEntityValue() != automationEntity)
                    {
                        SetReference(automationReference, automation);
                    }

                    return;
                }
            }

            ref StateAutomationLink newLink = ref links.Add();
            newLink.stateNameHash = stateNameHash;
            newLink.target = new(targetType, byteOffset);
            newLink.automationReference = AddReference(automation);
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update a specific field on component <typeparamref name="T"/>.
        /// </summary>
        public readonly void AddOrSetLinkToComponent<T>(ASCIIText256 stateName, AutomationEntity automation, ASCIIText256 fieldName) where T : unmanaged
        {
            Types.Type type = TypeRegistry.GetType<T>();
            int byteOffset = 0;
            long fieldNameHash = fieldName.GetLongHashCode();
            ReadOnlySpan<Field> variables = type.Fields;
            for (int i = 0; i < variables.Length; i++)
            {
                Field variable = variables[i];
                if (variable.Name.GetLongHashCode() == fieldNameHash)
                {
                    AddOrSetLinkToComponent<T>(stateName, automation, byteOffset);
                    return;
                }

                byteOffset += variable.Size;
            }

            throw new InvalidOperationException($"Field '{fieldName}' not found on component '{typeof(T).Name}'");
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update the array element <typeparamref name="T"/> at <paramref name="index"/>.
        /// </summary>
        public unsafe readonly void AddOrSetLinkToArrayElement<T>(ASCIIText256 stateName, AutomationEntity automation, int index, int byteOffset = 0) where T : unmanaged
        {
            StateMachine.ThrowIfStateIsMissing(stateName);

            int stateNameHash = stateName.GetHashCode();
            int bytePosition = index * sizeof(T) + byteOffset;
            Values<StateAutomationLink> links = GetArray<StateAutomationLink>();
            DataType targetType = world.Schema.GetArrayDataType<T>();
            int count = links.Length;
            for (int i = 0; i < count; i++)
            {
                ref StateAutomationLink existingLink = ref links[i];
                if (existingLink.stateNameHash == stateNameHash)
                {
                    rint automationReference = existingLink.automationReference;
                    uint automationEntity = GetReference(automationReference);
                    existingLink.target = new(targetType, bytePosition);
                    if (automation.GetEntityValue() != automationEntity)
                    {
                        SetReference(automationReference, automation);
                    }

                    return;
                }
            }

            ref StateAutomationLink newLink = ref links.Add();
            newLink.stateNameHash = stateNameHash;
            newLink.target = new(targetType, bytePosition);
            newLink.automationReference = AddReference(automation);
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update the array element <typeparamref name="T"/> at <paramref name="index"/>,
        /// specifically updating the field <paramref name="fieldName"/>.
        /// </summary>
        public readonly void AddOrSetLinkToArrayElement<T>(ASCIIText256 stateName, AutomationEntity automation, int index, ASCIIText256 fieldName) where T : unmanaged
        {
            Types.Type type = TypeRegistry.GetType<T>();
            int byteOffset = 0;
            long fieldNameHash = fieldName.GetLongHashCode();
            ReadOnlySpan<Field> variables = type.Fields;
            for (int i = 0; i < variables.Length; i++)
            {
                Field variable = variables[i];
                if (variable.Name.GetLongHashCode() == fieldNameHash)
                {
                    AddOrSetLinkToArrayElement<T>(stateName, automation, index, byteOffset);
                    return;
                }

                byteOffset += variable.Size;
            }

            throw new InvalidOperationException($"Field `{fieldName}` not found in `{typeof(T).Name}`");
        }

        public static implicit operator Stateful(StatefulAutomationPlayer entity)
        {
            return entity.As<Stateful>();
        }

        public static implicit operator AutomationPlayer(StatefulAutomationPlayer entity)
        {
            return entity.As<AutomationPlayer>();
        }
    }
}