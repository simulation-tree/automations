using System;
using Worlds;

namespace Automations.Components
{
    [Component]
    public struct IsAutomation
    {
        public ArrayElementType keyframeType;
        public InterpolationMethod interpolationMethod;
        public bool loop;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomation()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomation(ArrayElementType keyframeType, InterpolationMethod interpolationMethod, bool loop)
        {
            this.keyframeType = keyframeType;
            this.interpolationMethod = interpolationMethod;
            this.loop = loop;
        }
    }
}