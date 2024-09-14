using System;
using Unmanaged;

namespace Automations.Components
{
    public struct IsAutomation
    {
        public RuntimeType keyframeType;
        public bool loop;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomation()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomation(RuntimeType keyframeType, bool loop)
        {
            this.keyframeType = keyframeType;
            this.loop = loop;
        }
    }
}