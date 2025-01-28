using Automations.Systems;
using Simulation.Tests;
using Types;
using Worlds;

namespace Automations.Tests
{
    public abstract class AutomationTests : SimulationTests
    {
        static AutomationTests()
        {
            TypeRegistry.Load<AutomationsTypeBank>();
            TypeRegistry.Load<AutomationsTestsTypeBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<AutomationsSchemaBank>();
            schema.Load<AutomationsTestsSchemaBank>();
            return schema;
        }

        protected override void SetUp()
        {
            base.SetUp();
            simulator.AddSystem<StateMachineSystem>();
            simulator.AddSystem<StateAutomationSystem>();
            simulator.AddSystem<AutomationPlayingSystem>();
        }
    }
}