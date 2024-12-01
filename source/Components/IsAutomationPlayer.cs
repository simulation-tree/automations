using System;
using Worlds;

namespace Automations.Components
{
    [Component]
    public struct IsAutomationPlayer
    {
        public rint automationReference;

        /// <summary>
        /// The type of component on this entity being automated.
        /// </summary>
        public ComponentType componentType;
        public TimeSpan time;
        public bool paused;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomationPlayer()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomationPlayer(rint automationReference, ComponentType componentType)
        {
            this.automationReference = automationReference;
            this.componentType = componentType;
            time = TimeSpan.Zero;
            paused = false;
        }
    }
}