using System;

namespace Automations.Components
{
    public struct IsStateMachine
    {
        public int entryState;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsStateMachine()
        {
            throw new NotSupportedException();
        }
#endif

        public IsStateMachine(int entryState)
        {
            this.entryState = entryState;
        }
    }
}