using System.Numerics;
using Worlds;

namespace Automations.Tests
{
    public class AutomationEntityTests : AutomationTests
    {
        [Test]
        public void CreateAutomationWithKeyframes()
        {
            using World world = CreateWorld();
            Automation<Vector3> testAutomation = new(world,
            [
                (0, Vector3.Zero),
                (1f, Vector3.UnitX),
                (2f, Vector3.UnitY),
                (3f, Vector3.UnitZ),
                (4f, Vector3.One),
            ]);

            Assert.That(testAutomation.IsCompliant, Is.True);
            Assert.That(testAutomation.Count, Is.EqualTo(5));
            Assert.That(testAutomation[0].time, Is.EqualTo(0f));
            Assert.That(testAutomation[0].value, Is.EqualTo(Vector3.Zero));
            Assert.That(testAutomation[1].time, Is.EqualTo(1f));
            Assert.That(testAutomation[1].value, Is.EqualTo(Vector3.UnitX));
            Assert.That(testAutomation[2].time, Is.EqualTo(2f));
            Assert.That(testAutomation[2].value, Is.EqualTo(Vector3.UnitY));
            Assert.That(testAutomation[3].time, Is.EqualTo(3f));
            Assert.That(testAutomation[3].value, Is.EqualTo(Vector3.UnitZ));
            Assert.That(testAutomation[4].time, Is.EqualTo(4f));
            Assert.That(testAutomation[4].value, Is.EqualTo(Vector3.One));
        }
    }
}