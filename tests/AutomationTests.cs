using Automations.Components;
using Automations.Systems;
using Simulation;
using Simulation.Tests;
using System;
using System.Numerics;
using Worlds;

namespace Automations.Tests
{
    public class AutomationTests : SimulationTests
    {
        protected override void SetUp()
        {
            base.SetUp();
            ComponentType.Register<IsAutomation>();
            ComponentType.Register<IsAutomationPlayer>();
            ComponentType.Register<Position>();
            ArrayType.Register<KeyframeTime>();
            ArrayType.Register<KeyframeValue1>();
            ArrayType.Register<KeyframeValue2>();
            ArrayType.Register<KeyframeValue4>();
            ArrayType.Register<KeyframeValue8>();
            ArrayType.Register<KeyframeValue16>();
            ArrayType.Register<KeyframeValue32>();
            ArrayType.Register<KeyframeValue64>();
            ArrayType.Register<KeyframeValue128>();
            ArrayType.Register<KeyframeValue256>();
            Simulator.AddSystem<AutomationPlayingSystem>();
        }

        [Test]
        public void CreateAutomationWithKeyframes()
        {
            Automation<Vector3> testAutomation = new(World,
            [
                (0, Vector3.Zero),
                (1f, Vector3.UnitX),
                (2f, Vector3.UnitY),
                (3f, Vector3.UnitZ),
                (4f, Vector3.One),
            ]);

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

        [Test]
        public void MoveTransformAutomation()
        {
            Automation<Vector3> testAutomation = new(World, InterpolationMethod.Vector3Linear,
            [
                (0, Vector3.Zero),
                (1f, Vector3.UnitX),
                (2f, Vector3.UnitY),
                (3f, Vector3.UnitZ),
                (4f, Vector3.One),
            ]);

            Entity thingToMove = new(World);
            thingToMove.AddComponent<Position>();
            
            AutomationPlayer thingPlayer = thingToMove.Become<AutomationPlayer>();
            thingPlayer.SetAutomation<Position>(testAutomation);
            thingPlayer.Play();

            TimeSpan delta = TimeSpan.FromSeconds(0.1f);
            TimeSpan time = TimeSpan.Zero;
            while (time < TimeSpan.FromSeconds(5f))
            {
                Simulator.Update(delta);
                time += delta;
                Vector3 currentPosition = thingToMove.GetComponent<Position>().value;
                Console.WriteLine(currentPosition);
                if (time == TimeSpan.FromSeconds(0.5f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(0.5f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(0f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(0f).Within(0.01f));
                }
                else if (time == TimeSpan.FromSeconds(1f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(1f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(0f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(0f).Within(0.01f));
                }
                else if (time == TimeSpan.FromSeconds(1.5f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(0.5f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(0.5f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(0f).Within(0.01f));
                }
                else if (time == TimeSpan.FromSeconds(2f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(0f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(1f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(0f).Within(0.01f));
                }
                else if (time == TimeSpan.FromSeconds(2.5f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(0f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(0.5f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(0.5f).Within(0.01f));
                }
                else if (time == TimeSpan.FromSeconds(3f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(0f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(0f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(1f).Within(0.01f));
                }
                else if (time == TimeSpan.FromSeconds(3.5f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(0.5f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(0.5f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(1f).Within(0.01f));
                }
                else if (time == TimeSpan.FromSeconds(4f))
                {
                    Assert.That(currentPosition.X, Is.EqualTo(1f).Within(0.01f));
                    Assert.That(currentPosition.Y, Is.EqualTo(1f).Within(0.01f));
                    Assert.That(currentPosition.Z, Is.EqualTo(1f).Within(0.01f));
                }
            }

            Vector3 finalPosition = thingToMove.GetComponent<Position>().value;
            Assert.That(finalPosition.X, Is.EqualTo(1f).Within(0.01f));
            Assert.That(finalPosition.Y, Is.EqualTo(1f).Within(0.01f));
            Assert.That(finalPosition.Z, Is.EqualTo(1f).Within(0.01f));
        }
    }

    public struct Position
    {
        public Vector3 value;
    }
}
