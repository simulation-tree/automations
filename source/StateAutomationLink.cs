using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct StateAutomationLink
    {
        public int stateNameHash;
        public ComponentType componentType;
        public rint automationReference;

#if NET
        [Obsolete("Default constructor not available", true)]
        public StateAutomationLink()
        {
            throw new NotSupportedException();
        }
#endif

        public StateAutomationLink(FixedString stateName, ComponentType componentType, rint automationReference)
        {
            this.stateNameHash = stateName.GetHashCode();
            this.componentType = componentType;
            this.automationReference = automationReference;
        }
    }
}