using Automations.Components;
using Simulation;
using System;
using System.Diagnostics;
using Unmanaged;

namespace Automations
{
    public readonly struct AutomationPlayer : IEntity
    {
        public readonly Entity entity;

        public readonly ref bool IsPaused => ref entity.GetComponentRef<IsAutomationPlayer>().paused;
        public readonly ref TimeSpan Time => ref entity.GetComponentRef<IsAutomationPlayer>().time;
        public readonly ref RuntimeType ComponentType => ref entity.GetComponentRef<IsAutomationPlayer>().componentType;

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
        readonly Definition IEntity.Definition => new([RuntimeType.Get<IsAutomationPlayer>()], []);

        /// <summary>
        /// Creates a new automation player.
        /// </summary>
        public AutomationPlayer(World world)
        {
            this.entity = new(world);
            entity.AddComponent<IsAutomationPlayer>();
        }

        public readonly void SetAutomation<T>(Automation automation) where T : unmanaged
        {
            ref IsAutomationPlayer player = ref entity.GetComponentRef<IsAutomationPlayer>();
            player.time = TimeSpan.Zero;
            player.componentType = RuntimeType.Get<T>();
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
    }
}