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
            ArrayType.Register<Keyframe<Vector3>>();
            ArrayType.Register<Keyframe<Vector4>>();
            Simulator.AddSystem<AutomationPlayingSystem>();
        }

        [Test]
        public void MoveTransformAutomation()
        {
            Automation<Vector3> testAutomation = new(World,
            [
                new Keyframe<Vector3>(0, Vector3.Zero),
                new Keyframe<Vector3>(1f, Vector3.UnitX),
                new Keyframe<Vector3>(2f, Vector3.UnitY),
                new Keyframe<Vector3>(3f, Vector3.UnitZ),
                new Keyframe<Vector3>(4f, Vector3.One),
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
