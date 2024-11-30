using Automations.Components;
using System;
using Worlds;

namespace Automations
{
    public readonly struct AutomationPlayer : IEntity
    {
        private readonly Entity entity;

        public readonly ref bool IsPaused => ref entity.GetComponentRef<IsAutomationPlayer>().paused;
        public readonly ref TimeSpan Time => ref entity.GetComponentRef<IsAutomationPlayer>().time;
        public readonly ref ComponentType ComponentType => ref entity.GetComponentRef<IsAutomationPlayer>().componentType;

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
        readonly Definition IEntity.Definition => new Definition().AddComponentType<IsAutomationPlayer>();

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
            ref IsAutomationPlayer player = ref entity.GetComponentRef<IsAutomationPlayer>();
            player.time = TimeSpan.Zero;
            player.componentType = ComponentType.Get<T>();
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
            ref IsAutomationPlayer player = ref entity.GetComponentRef<IsAutomationPlayer>();
            player.paused = true;
        }

        public readonly void Play()
        {
            ref IsAutomationPlayer player = ref entity.GetComponentRef<IsAutomationPlayer>();
            player.paused = false;
        }

        public static implicit operator Entity(AutomationPlayer player)
        {
            return player.entity;
        }
    }
}