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

        public readonly FixedString CurrentState
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

        public readonly ref float AddParameter(FixedString name, float defaultValue = 0f)
        {
            Stateful stateful = As<Stateful>();
            return ref stateful.AddParameter(name, defaultValue);
        }

        public readonly ref float GetParameterRef(FixedString name)
        {
            Stateful stateful = As<Stateful>();
            return ref stateful.GetParameterRef(name);
        }

        public readonly bool ContainsParameter(FixedString name)
        {
            Stateful stateful = As<Stateful>();
            return stateful.ContainsParameter(name);
        }

        public readonly void SetParameter(FixedString name, float value)
        {
            Stateful stateful = As<Stateful>();
            stateful.SetParameter(name, value);
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update component <typeparamref name="T"/>.
        /// </summary>
        public readonly void AddOrSetLinkToComponent<T>(FixedString stateName, AutomationEntity automation, uint byteOffset = 0) where T : unmanaged
        {
            StateMachine.ThrowIfStateIsMissing(stateName);

            int stateNameHash = stateName.GetHashCode();
            USpan<StateAutomationLink> links = GetArray<StateAutomationLink>();
            DataType targetType = world.Schema.GetComponentDataType<T>();
            uint count = links.Length;
            for (uint i = 0; i < count; i++)
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

            links = ResizeArray<StateAutomationLink>(count + 1);
            ref StateAutomationLink newLink = ref links[count];
            newLink.stateNameHash = stateNameHash;
            newLink.target = new(targetType, byteOffset);
            newLink.automationReference = AddReference(automation);
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update a specific field on component <typeparamref name="T"/>.
        /// </summary>
        public readonly void AddOrSetLinkToComponent<T>(FixedString stateName, AutomationEntity automation, FixedString fieldName) where T : unmanaged
        {
            TypeLayout type = TypeRegistry.Get<T>();
            uint byteOffset = 0;
            for (uint i = 0; i < type.Count; i++)
            {
                TypeLayout.Variable variable = type[i];
                if (variable.Name == fieldName)
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
        /// bound to update the array element <typeparamref name="T"/> at <paramref name="arrayIndex"/>.
        /// </summary>
        public unsafe readonly void AddOrSetLinkToArrayElement<T>(FixedString stateName, AutomationEntity automation, uint arrayIndex, uint byteOffset = 0) where T : unmanaged
        {
            StateMachine.ThrowIfStateIsMissing(stateName);

            int stateNameHash = stateName.GetHashCode();
            uint bytePosition = arrayIndex * (uint)sizeof(T) + byteOffset;
            USpan<StateAutomationLink> links = GetArray<StateAutomationLink>();
            DataType targetType = world.Schema.GetArrayElementDataType<T>();
            uint count = links.Length;
            for (uint i = 0; i < count; i++)
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

            links = ResizeArray<StateAutomationLink>(count + 1);
            ref StateAutomationLink newLink = ref links[count];
            newLink.stateNameHash = stateNameHash;
            newLink.target = new(targetType, bytePosition);
            newLink.automationReference = AddReference(automation);
        }

        /// <summary>
        /// Adds or updates a link between a state and an automation
        /// bound to update the array element <typeparamref name="T"/> at <paramref name="arrayIndex"/>,
        /// specifically updating the field <paramref name="fieldName"/>.
        /// </summary>
        public readonly void AddOrSetLinkToArrayElement<T>(FixedString stateName, AutomationEntity automation, uint arrayIndex, FixedString fieldName) where T : unmanaged
        {
            TypeLayout type = TypeRegistry.Get<T>();
            uint byteOffset = 0;
            for (uint i = 0; i < type.Count; i++)
            {
                TypeLayout.Variable variable = type[i];
                if (variable.Name == fieldName)
                {
                    AddOrSetLinkToArrayElement<T>(stateName, automation, arrayIndex, byteOffset);
                    return;
                }

                byteOffset += variable.Size;
            }

            throw new InvalidOperationException($"Field '{fieldName}' not found on array element '{typeof(T).Name}'");
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