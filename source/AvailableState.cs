using System;
using Unmanaged;

namespace Automations
{
    public struct AvailableState
    {
        public FixedString name;

#if NET
        [Obsolete("Default constructor not available", true)]
        public AvailableState()
        {
            throw new NotSupportedException();
        }
#endif

        public AvailableState(FixedString name)
        {
            this.name = name;
        }
    }
}