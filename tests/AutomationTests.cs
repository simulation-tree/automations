﻿using Automations.Components;
using Automations.Systems;
using Simulation.Tests;
using System;
using System.Numerics;
using Types;
using Worlds;

namespace Automations.Tests
{
    public class AutomationTests : SimulationTests
    {
        static AutomationTests()
        {
            TypeLayout.Register<IsAutomation>();
            TypeLayout.Register<IsStateful>();
            TypeLayout.Register<IsStateMachine>();
            TypeLayout.Register<IsAutomationPlayer>();
            TypeLayout.Register<Position>();
            TypeLayout.Register<AvailableState>();
            TypeLayout.Register<Transition>();
            TypeLayout.Register<Parameter>();
            TypeLayout.Register<StateAutomationLink>();
            TypeLayout.Register<KeyframeTime>();
            TypeLayout.Register<KeyframeValue1>();
            TypeLayout.Register<KeyframeValue2>();
            TypeLayout.Register<KeyframeValue4>();
            TypeLayout.Register<KeyframeValue8>();
            TypeLayout.Register<KeyframeValue16>();
            TypeLayout.Register<KeyframeValue32>();
            TypeLayout.Register<KeyframeValue64>();
            TypeLayout.Register<KeyframeValue128>();
            TypeLayout.Register<KeyframeValue256>();
        }

        protected override void SetUp()
        {
            base.SetUp();
            world.Schema.RegisterComponent<IsAutomation>();
            world.Schema.RegisterComponent<IsStateful>();
            world.Schema.RegisterComponent<IsStateMachine>();
            world.Schema.RegisterComponent<IsAutomationPlayer>();
            world.Schema.RegisterComponent<Position>();
            world.Schema.RegisterArrayElement<AvailableState>();
            world.Schema.RegisterArrayElement<Transition>();
            world.Schema.RegisterArrayElement<Parameter>();
            world.Schema.RegisterArrayElement<StateAutomationLink>();
            world.Schema.RegisterArrayElement<KeyframeTime>();
            world.Schema.RegisterArrayElement<KeyframeValue1>();
            world.Schema.RegisterArrayElement<KeyframeValue2>();
            world.Schema.RegisterArrayElement<KeyframeValue4>();
            world.Schema.RegisterArrayElement<KeyframeValue8>();
            world.Schema.RegisterArrayElement<KeyframeValue16>();
            world.Schema.RegisterArrayElement<KeyframeValue32>();
            world.Schema.RegisterArrayElement<KeyframeValue64>();
            world.Schema.RegisterArrayElement<KeyframeValue128>();
            world.Schema.RegisterArrayElement<KeyframeValue256>();
            simulator.AddSystem<AutomationPlayingSystem>();
        }

        [Test]
        public void CreateAutomationWithKeyframes()
        {
            Automation<Vector3> testAutomation = new(world,
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
            Automation<Vector3> testAutomation = new(world, InterpolationMethod.Vector3Linear,
            [
                (0, Vector3.Zero),
                (1f, Vector3.UnitX),
                (2f, Vector3.UnitY),
                (3f, Vector3.UnitZ),
                (4f, Vector3.One),
            ]);

            Entity thingToMove = new(world);
            thingToMove.AddComponent<Position>();

            AutomationPlayer thingPlayer = thingToMove.Become<AutomationPlayer>();
            thingPlayer.SetAutomation<Position>(testAutomation);
            thingPlayer.Play();

            TimeSpan delta = TimeSpan.FromSeconds(0.1f);
            TimeSpan time = TimeSpan.Zero;
            while (time < TimeSpan.FromSeconds(5f))
            {
                simulator.Update(delta);
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
