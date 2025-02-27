using Automations.Components;
using System;
using Types;
using Unmanaged;
using Worlds;

namespace Automations
{
    public readonly partial struct AutomationPlayer : IEntity
    {
        public readonly ref bool IsPaused => ref GetComponent<IsAutomationPlayer>().paused;
        public readonly ref TimeSpan Time => ref GetComponent<IsAutomationPlayer>().time;

        public readonly AutomationEntity CurrentAutomation
        {
            get
            {
                rint automationReference = GetComponent<IsAutomationPlayer>().automationReference;
                uint automationEntity = GetReference(automationReference);
                return new Entity(world, automationEntity).As<AutomationEntity>();
            }
        }

        /// <summary>
        /// Creates a new automation player.
        /// </summary>
        public AutomationPlayer(World world)
        {
            this.world = world;
            value = world.CreateEntity(new IsAutomationPlayer(default, default));
        }

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsAutomationPlayer>();
        }

        /// <summary>
        /// Assigns the given <paramref name="automation"/> to mutate a component of
        /// type <typeparamref name="T"/>.
        /// </summary>
        public readonly void SetAutomationForComponent<T>(AutomationEntity automation, uint byteOffset = 0) where T : unmanaged
        {
            ref IsAutomationPlayer player = ref GetComponent<IsAutomationPlayer>();
            player.time = TimeSpan.Zero;
            player.target = new(world.Schema.GetComponentDataType<T>(), byteOffset);
            if (player.automationReference != default)
            {
                SetReference(player.automationReference, automation);
            }
            else
            {
                player.automationReference = AddReference(automation);
            }
        }

        /// <summary>
        /// Assigns the given <paramref name="automation"/> to mutate the field with name <paramref name="fieldName"/>
        /// of a <typeparamref name="T"/> component.
        /// </summary>
        public readonly void SetAutomationForComponent<T>(AutomationEntity automation, FixedString fieldName) where T : unmanaged
        {
            TypeLayout type = TypeRegistry.Get<T>();
            uint byteOffset = 0;
            for (uint i = 0; i < type.Count; i++)
            {
                TypeLayout.Variable variable = type[i];
                if (variable.Name == fieldName)
                {
                    SetAutomationForComponent<T>(automation, byteOffset);
                    return;
                }

                byteOffset += variable.Size;
            }

            throw new InvalidOperationException($"Field '{fieldName}' not found on array element '{typeof(T).Name}'");
        }

        /// <summary>
        /// Assigns the given <paramref name="automation"/> to mutate an array element of
        /// type <typeparamref name="T"/>.
        /// </summary>
        public unsafe readonly void SetAutomationForArrayElement<T>(AutomationEntity automation, uint arrayIndex, uint byteOffset = 0) where T : unmanaged
        {
            ref IsAutomationPlayer player = ref GetComponent<IsAutomationPlayer>();
            player.time = TimeSpan.Zero;
            uint bytePosition = arrayIndex * (uint)sizeof(T) + byteOffset;
            player.target = new(world.Schema.GetArrayDataType<T>(), bytePosition);
            if (player.automationReference != default)
            {
                SetReference(player.automationReference, automation);
            }
            else
            {
                player.automationReference = AddReference(automation);
            }
        }

        /// <summary>
        /// Assigns the given <paramref name="automation"/> to mutate the field with name <paramref name="fieldName"/>
        /// of a <typeparamref name="T"/> array element at <paramref name="arrayIndex"/>.
        /// </summary>
        public readonly void SetAutomationForArrayElement<T>(AutomationEntity automation, uint arrayIndex, FixedString fieldName) where T : unmanaged
        {
            TypeLayout type = TypeRegistry.Get<T>();
            uint byteOffset = 0;
            for (uint i = 0; i < type.Count; i++)
            {
                TypeLayout.Variable variable = type[i];
                if (variable.Name == fieldName)
                {
                    SetAutomationForArrayElement<T>(automation, arrayIndex, byteOffset);
                    return;
                }

                byteOffset += variable.Size;
            }

            throw new InvalidOperationException($"Field '{fieldName}' not found on array element '{typeof(T).Name}'");
        }

        public readonly void Pause()
        {
            ref IsAutomationPlayer player = ref GetComponent<IsAutomationPlayer>();
            player.paused = true;
        }

        public readonly void Play()
        {
            ref IsAutomationPlayer player = ref GetComponent<IsAutomationPlayer>();
            player.paused = false;
        }
    }
}