using Worlds;

namespace Automations
{
    public readonly struct AutomationTarget
    {
        public readonly DataType targetType;
        public readonly int bytePosition;

        public AutomationTarget(DataType targetType, int bytePosition)
        {
            this.targetType = targetType;
            this.bytePosition = bytePosition;
        }
    }
}