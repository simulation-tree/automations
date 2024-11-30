using System;
using Worlds;

namespace Automations.Components
{
    [Component]
    public struct IsStateful
    {
        public uint state;
        public rint stateMachineReference;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsStateful()
        {
            throw new NotSupportedException();
        }
#endif

        public IsStateful(uint state, rint stateMachineReference)
        {
            this.state = state;
            this.stateMachineReference = stateMachineReference;
        }
    }
}