using System;

namespace Automations.Components
{
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