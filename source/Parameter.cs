using System;
using Unmanaged;

namespace Automations
{
    public struct Parameter
    {
        public FixedString name;
        public float value;

#if NET
        [Obsolete("Default constructor not available", true)]
        public Parameter()
        {
            throw new NotSupportedException();
        }
#endif

        public Parameter(FixedString name, float value)
        {
            this.name = name;
            this.value = value;
        }
    }
}