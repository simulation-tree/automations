using System;
using Unmanaged;
using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct Transition
    {
        public int sourceStateHash;
        public int destinationStateHash;
        public int parameterHash;
        public Condition condition;
        public float value;

#if NET
        [Obsolete("Default constructor not available", true)]
        public Transition()
        {
            throw new NotSupportedException();
        }
#endif

        public Transition(FixedString sourceState, FixedString destinationState, FixedString parameter, Condition condition, float value)
        {
            this.sourceStateHash = sourceState.GetHashCode();
            this.destinationStateHash = destinationState.GetHashCode();
            this.parameterHash = parameter.GetHashCode();
            this.condition = condition;
            this.value = value;
        }

        public enum Condition : byte
        {
            None,
            Equal,
            NotEqual,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual
        }
    }
}