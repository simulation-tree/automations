using Simulation;
using System;
using Unmanaged;

namespace Automations.Components
{
    public struct IsAutomationPlayer
    {
        public rint automationReference;
        public RuntimeType componentType;
        public TimeSpan time;
        public bool paused;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomationPlayer()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomationPlayer(rint automationReference, RuntimeType componentType)
        {
            this.automationReference = automationReference;
            this.componentType = componentType;
            time = TimeSpan.Zero;
            paused = false;
        }
    }
}