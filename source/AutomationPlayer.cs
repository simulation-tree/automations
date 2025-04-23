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
        public readonly void SetAutomationForComponent<T>(AutomationEntity automation, int byteOffset = 0) where T : unmanaged
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
        public readonly void SetAutomationForComponent<T>(AutomationEntity automation, ASCIIText256 fieldName) where T : unmanaged
        {
            TypeMetadata type = MetadataRegistry.GetType<T>();
            int byteOffset = 0;
            long fieldNameHash = fieldName.GetLongHashCode();
            ReadOnlySpan<Field> variables = type.Fields;
            for (int i = 0; i < variables.Length; i++)
            {
                Field variable = variables[i];
                if (variable.Name.GetLongHashCode() == fieldNameHash)
                {
                    SetAutomationForComponent<T>(automation, byteOffset);
                    return;
                }

                byteOffset += variable.Size;
            }

            throw new InvalidOperationException($"Field `{fieldName}` not found in `{typeof(T).Name}`");
        }

        /// <summary>
        /// Assigns the given <paramref name="automation"/> to mutate an array element of
        /// type <typeparamref name="T"/>.
        /// </summary>
        public unsafe readonly void SetAutomationForArrayElement<T>(AutomationEntity automation, int index, int byteOffset = 0) where T : unmanaged
        {
            ref IsAutomationPlayer player = ref GetComponent<IsAutomationPlayer>();
            player.time = TimeSpan.Zero;
            int bytePosition = index * sizeof(T) + byteOffset;
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
        /// of a <typeparamref name="T"/> array element at <paramref name="index"/>.
        /// </summary>
        public readonly void SetAutomationForArrayElement<T>(AutomationEntity automation, int index, ASCIIText256 fieldName) where T : unmanaged
        {
            TypeMetadata type = MetadataRegistry.GetType<T>();
            int byteOffset = 0;
            long fieldNameHash = fieldName.GetLongHashCode();
            ReadOnlySpan<Field> variables = type.Fields;
            for (int i = 0; i < variables.Length; i++)
            {
                Field variable = variables[i];
                if (variable.Name.GetLongHashCode() == fieldNameHash)
                {
                    SetAutomationForArrayElement<T>(automation, index, byteOffset);
                    return;
                }

                byteOffset += variable.Size;
            }

            throw new InvalidOperationException($"Field `{fieldName}` not found on in `{typeof(T).Name}`");
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