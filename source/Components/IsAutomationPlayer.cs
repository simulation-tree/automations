using System;
using Worlds;

namespace Automations.Components
{
    [Component]
    public struct IsAutomationPlayer
    {
        public rint automationReference;

        /// <summary>
        /// The type of component or array on this entity being automated.
        /// </summary>
        public DataType dataType;

        public uint arrayIndex;
        public TimeSpan time;
        public bool paused;

#if NET
        [Obsolete("Default constructor not available", true)]
        public IsAutomationPlayer()
        {
            throw new NotSupportedException();
        }
#endif

        public IsAutomationPlayer(rint automationReference, DataType dataType)
        {
            this.automationReference = automationReference;
            this.dataType = dataType;
            time = TimeSpan.Zero;
            paused = false;
        }
    }
}