using Automations.Components;
using System;
using Worlds;

namespace Automations
{
    public readonly struct AutomationPlayer : IEntity
    {
        private readonly Entity entity;

        public readonly ref bool IsPaused => ref entity.GetComponent<IsAutomationPlayer>().paused;
        public readonly ref TimeSpan Time => ref entity.GetComponent<IsAutomationPlayer>().time;
        public readonly ref DataType ComponentType => ref entity.GetComponent<IsAutomationPlayer>().componentType;

        public readonly Automation CurrentAutomation
        {
            get
            {
                rint automationReference = entity.GetComponent<IsAutomationPlayer>().automationReference;
                uint automationEntity = entity.GetReference(automationReference);
                return new(entity.world, automationEntity);
            }
        }

        readonly uint IEntity.Value => entity.value;
        readonly World IEntity.World => entity.world;

        readonly void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsAutomationPlayer>();
        }

        /// <summary>
        /// Creates a new automation player.
        /// </summary>
        public AutomationPlayer(World world)
        {
            this.entity = new(world);
            entity.AddComponent<IsAutomationPlayer>();
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public readonly void SetAutomation<T>(Automation automation) where T : unmanaged
        {
            ref IsAutomationPlayer player = ref entity.GetComponent<IsAutomationPlayer>();
            player.time = TimeSpan.Zero;
            player.componentType = entity.GetWorld().Schema.GetComponentDataType<T>();
            if (player.automationReference != default)
            {
                entity.SetReference(player.automationReference, automation);
            }
            else
            {
                player.automationReference = entity.AddReference(automation);
            }
        }

        public readonly void Pause()
        {
            ref IsAutomationPlayer player = ref entity.GetComponent<IsAutomationPlayer>();
            player.paused = true;
        }

        public readonly void Play()
        {
            ref IsAutomationPlayer player = ref entity.GetComponent<IsAutomationPlayer>();
            player.paused = false;
        }

        public static implicit operator Entity(AutomationPlayer player)
        {
            return player.entity;
        }
    }
}