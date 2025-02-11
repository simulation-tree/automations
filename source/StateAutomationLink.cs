using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct StateAutomationLink
    {
        public int stateNameHash;
        public DataType targetType;
        public uint arrayIndex;
        public rint automationReference;

#if NET
        [Obsolete("Default constructor not available", true)]
        public StateAutomationLink()
        {
            throw new NotSupportedException();
        }
#endif

        public StateAutomationLink(FixedString stateName, DataType targetType, rint automationReference)
        {
            this.stateNameHash = stateName.GetHashCode();
            this.targetType = targetType;
            this.automationReference = automationReference;
        }
    }
}