using Worlds;

namespace Automations
{
    public readonly struct AutomationTarget
    {
        public readonly DataType targetType;
        public readonly uint bytePosition;

        public AutomationTarget(DataType targetType, uint bytePosition)
        {
            this.targetType = targetType;
            this.bytePosition = bytePosition;
        }
    }
}