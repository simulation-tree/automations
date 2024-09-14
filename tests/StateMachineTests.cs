using Automations.Events;
using Automations.Systems;
using Simulation;
using System;

namespace Automations.Tests
{
    public class StateMachineTests
    {
        private void Simulate(World world, TimeSpan delta)
        {
            world.Submit(new StateUpdate());
            world.Submit(new AutomationUpdate(delta));
            world.Poll();
        }

        [Test]
        public void SimpleStateMachine()
        {
            using World world = new();
            using AutomationPlayingSystem automations = new(world);
            using StateMachineSystem stateMachines = new(world);

            StateMachine machine = new(world);
            Assert.Throws<InvalidOperationException>(() => Console.WriteLine(machine.EntryState));

            machine.AddState("Entry State");
            machine.AddState("Other State");
            machine.AddTransition("Entry State", "Other State", "pastrami", Transition.Condition.GreaterThan, 0f);
            machine.EntryState = "Entry State";

            Assert.That(machine.EntryState.ToString(), Is.EqualTo("Entry State"));

            Entity entity = new(world);
            entity.AddComponent(0f);
            Stateful stateful = entity.Become<Stateful>();
            stateful.StateMachine = machine;
            stateful.AddParameter("pastrami", 0f);

            Simulate(world, TimeSpan.FromSeconds(1f));

            Assert.That(stateful.CurrentState.ToString(), Is.EqualTo("Entry State"));

            stateful.SetParameter("pastrami", 0.05f);

            Simulate(world, TimeSpan.FromSeconds(1f));

            Assert.That(stateful.CurrentState.ToString(), Is.EqualTo("Other State"));
        }

        [Test]
        public void StatefulEntityWithAutomations()
        {
            using World world = new();
            using AutomationPlayingSystem automations = new(world);
            using StateMachineSystem stateMachines = new(world);
            using StateAutomationSystem stateAutomation = new(world);

            Automation<float> defaultAutomation = new(world, [new(0f, 0f)]);
            Automation<float> triangleWave = new(world, [new(0f, 0f), new(1f, 1f), new(2f, 0f)], true);
            StateMachine machine = new(world);
            machine.AddState("Entry State");
            machine.AddState("Other State");
            machine.AddTransition("Entry State", "Other State", "pastrami", Transition.Condition.GreaterThan, 0f);
            machine.AddTransition("Other State", "Entry State", "pastrami", Transition.Condition.LessThanOrEqual, 0f);
            machine.EntryState = "Entry State";

            Assert.That(machine.EntryState.ToString(), Is.EqualTo("Entry State"));

            Entity entity = new(world);
            entity.AddComponent(0f);
            StatefulAutomationPlayer stateful = entity.Become<StatefulAutomationPlayer>();
            stateful.StateMachine = machine;
            stateful.AddParameter("pastrami", 0f);
            stateful.AddOrSetLink<float>("Entry State", defaultAutomation);
            stateful.AddOrSetLink<float>("Other State", triangleWave);

            Assert.That(stateful.CurrentState.ToString(), Is.EqualTo("Entry State"));

            Simulate(world, TimeSpan.FromSeconds(0.1f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0f).Within(0.01f));

            stateful.SetParameter("pastrami", 1f);
            Simulate(world, TimeSpan.FromSeconds(0.1f));
            Simulate(world, TimeSpan.FromSeconds(0.1f));
            Simulate(world, TimeSpan.FromSeconds(0.1f));
            Simulate(world, TimeSpan.FromSeconds(0.1f));
            Simulate(world, TimeSpan.FromSeconds(0.1f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0.5f).Within(0.01f));

            Simulate(world, TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(1f).Within(0.01f));

            Simulate(world, TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0.5f).Within(0.01f));

            stateful.SetParameter("pastrami", 0f);
            Simulate(world, TimeSpan.FromSeconds(0.1f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0f).Within(0.01f));
        }
    }
}
