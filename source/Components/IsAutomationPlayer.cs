using System;
using Worlds;

namespace Automations.Components
{
    public struct IsAutomationPlayer
    {
        public rint automationReference;
        public AutomationTarget target;
        public double time;
        public bool paused;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomationPlayer()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomationPlayer(rint automationReference, AutomationTarget target)
        {
            this.automationReference = automationReference;
            this.target = target;
            time = 0;
            paused = false;
        }
    }
}