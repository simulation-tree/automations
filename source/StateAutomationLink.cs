using Simulation;
using System;
using Unmanaged;

namespace Automations
{
    public struct StateAutomationLink
    {
        public int stateNameHash;
        public RuntimeType componentType;
        public rint automationReference;

#if NET
        [Obsolete("Default constructor not available", true)]
        public StateAutomationLink()
        {
            throw new NotSupportedException();
        }
#endif

        public StateAutomationLink(FixedString stateName, RuntimeType componentType, rint automationReference)
        {
            this.stateNameHash = stateName.GetHashCode();
            this.componentType = componentType;
            this.automationReference = automationReference;
        }
    }
}