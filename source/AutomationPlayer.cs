using Automations.Components;
using System;
using Worlds;

namespace Automations
{
    public readonly partial struct AutomationPlayer : IEntity
    {
        public readonly ref bool IsPaused => ref GetComponent<IsAutomationPlayer>().paused;
        public readonly ref TimeSpan Time => ref GetComponent<IsAutomationPlayer>().time;
        public readonly ref DataType ComponentType => ref GetComponent<IsAutomationPlayer>().componentType;

        public readonly Automation CurrentAutomation
        {
            get
            {
                rint automationReference = GetComponent<IsAutomationPlayer>().automationReference;
                uint automationEntity = GetReference(automationReference);
                return new Entity(world, automationEntity).As<Automation>();
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

        public readonly void SetAutomation<T>(Automation automation) where T : unmanaged
        {
            ref IsAutomationPlayer player = ref GetComponent<IsAutomationPlayer>();
            player.time = TimeSpan.Zero;
            player.componentType = world.Schema.GetComponentDataType<T>();
            if (player.automationReference != default)
            {
                SetReference(player.automationReference, automation);
            }
            else
            {
                player.automationReference = AddReference(automation);
            }
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