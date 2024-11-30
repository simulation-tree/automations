using System;
using Worlds;

namespace Automations.Components
{
    [Component]
    public struct IsAutomation
    {
        public ArrayType keyframeType;
        public bool loop;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomation()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomation(ArrayType keyframeType, bool loop)
        {
            this.keyframeType = keyframeType;
            this.loop = loop;
        }
    }
}