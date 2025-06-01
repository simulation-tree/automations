namespace Automations.Messages
{
    public readonly struct AutomationUpdate
    {
        public readonly double deltaTime;

        public AutomationUpdate(double deltaTime)
        {
            this.deltaTime = deltaTime;
        }
    }
}
