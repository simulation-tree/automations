using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    public struct StateAutomationLink
    {
        public int stateNameHash;
        public AutomationTarget target;
        public rint automationReference;

#if NET
        [Obsolete("Default constructor not available", true)]
        public StateAutomationLink()
        {
            throw new NotSupportedException();
        }
#endif

        public StateAutomationLink(FixedString stateName, AutomationTarget target, rint automationReference)
        {
            this.stateNameHash = stateName.GetHashCode();
            this.target = target;
            this.automationReference = automationReference;
        }
    }
}