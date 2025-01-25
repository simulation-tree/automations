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
            TypeRegistry.Load<Automations.TypeBank>();
            TypeRegistry.Load<Automations.Tests.TypeBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<Automations.SchemaBank>();
            schema.Load<Automations.Tests.SchemaBank>();
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