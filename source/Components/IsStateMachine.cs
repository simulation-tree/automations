using System;
using Worlds;

namespace Automations.Components
{
    [Component]
    public struct IsStateMachine
    {
        public uint entryState;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsStateMachine()
        {
            throw new NotSupportedException();
        }
#endif

        public IsStateMachine(uint entryState)
        {
            this.entryState = entryState;
        }
    }
}