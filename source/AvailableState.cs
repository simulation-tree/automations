using System;
using Unmanaged;

namespace Automations
{
    public struct AvailableState
    {
        public ASCIIText256 name;

#if NET
        [Obsolete("Default constructor not available", true)]
        public AvailableState()
        {
            throw new NotSupportedException();
        }
#endif

        public AvailableState(ASCIIText256 name)
        {
            this.name = name;
        }
    }
}