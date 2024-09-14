using System;

namespace Automations.Events
{
    public readonly struct AutomationUpdate
    {
        public readonly TimeSpan delta;

#if NET
        [Obsolete("Default constructor not available", true)]
        public AutomationUpdate()
        {
            throw new NotSupportedException();
        }
#endif

        public AutomationUpdate(TimeSpan delta)
        {
            this.delta = delta;
        }
    }
}