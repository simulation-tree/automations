namespace Automations
{
    public unsafe readonly struct TransitionConditionFunction
    {
#if NET
        private readonly delegate* unmanaged<Parameter, uint> function;

        public TransitionConditionFunction(delegate* unmanaged<Parameter, uint> function)
        {
            this.function = function;
        }
#else
        private readonly delegate*<Parameter, uint> function;

        public TransitionConditionFunction(delegate*<Parameter, uint> function)
        {
            this.function = function;
        }
#endif

        public readonly uint Invoke(Parameter parameter)
        {
            return function(parameter);
        }
    }
}