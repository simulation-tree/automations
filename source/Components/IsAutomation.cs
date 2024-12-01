using System;
using Worlds;

namespace Automations.Components
{
    [Component]
    public struct IsAutomation
    {
        public ArrayType keyframeType;
        public InterpolationMethod interpolationMethod;
        public bool loop;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomation()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomation(ArrayType keyframeType, InterpolationMethod interpolationMethod, bool loop)
        {
            this.keyframeType = keyframeType;
            this.interpolationMethod = interpolationMethod;
            this.loop = loop;
        }
    }
}