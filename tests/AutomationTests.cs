using Types;
using Worlds;
using Worlds.Tests;

namespace Automations.Tests
{
    public abstract class AutomationTests : WorldTests
    {
        static AutomationTests()
        {
            TypeRegistry.Load<AutomationsTypeBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<AutomationsSchemaBank>();
            return schema;
        }
    }
}