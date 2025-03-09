using System;
using Worlds;

namespace Automations.Components
{
    public struct IsStateful
    {
        public int state;
        public rint stateMachineReference;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsStateful()
        {
            throw new NotSupportedException();
        }
#endif

        public IsStateful(int state, rint stateMachineReference)
        {
            this.state = state;
            this.stateMachineReference = stateMachineReference;
        }
    }
}