using Automations.Components;
using Automations.Systems;
using Simulation;
using Simulation.Tests;
using System;
using Worlds;

namespace Automations.Tests
{
    public class StateMachineTests : SimulationTests
    {
        protected override void SetUp()
        {
            base.SetUp();
            ComponentType.Register<IsStateful>();
            ComponentType.Register<IsStateMachine>();
            ComponentType.Register<IsAutomation>();
            ComponentType.Register<IsAutomationPlayer>();
            ComponentType.Register<Position>();
            ComponentType.Register<float>();
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
            ArrayType.Register<AvailableState>();
            ArrayType.Register<Transition>();
            ArrayType.Register<Parameter>();
            ArrayType.Register<StateAutomationLink>();
            Simulator.AddSystem<StateMachineSystem>();
            Simulator.AddSystem<StateAutomationSystem>();
            Simulator.AddSystem<AutomationPlayingSystem>();
        }

        [Test]
        public void SimpleStateMachine()
        {
            StateMachine machine = new(World);
            Assert.Throws<InvalidOperationException>(() => Console.WriteLine(machine.EntryState));

            machine.AddState("Entry State");
            machine.AddState("Other State");
            machine.AddTransition("Entry State", "Other State", "pastrami", Transition.Condition.GreaterThan, 0f);
            machine.EntryState = "Entry State";

            Assert.That(machine.EntryState.ToString(), Is.EqualTo("Entry State"));

            Entity entity = new(World);
            entity.AddComponent(0f);
            Stateful stateful = entity.Become<Stateful>();
            stateful.StateMachine = machine;
            stateful.AddParameter("pastrami", 0f);

            Simulator.Update(TimeSpan.FromSeconds(1f));

            Assert.That(stateful.CurrentState.ToString(), Is.EqualTo("Entry State"));

            stateful.SetParameter("pastrami", 0.05f);

            Simulator.Update(TimeSpan.FromSeconds(1f));

            Assert.That(stateful.CurrentState.ToString(), Is.EqualTo("Other State"));
        }

        [Test]
        public void StatefulEntityWithAutomations()
        {
            Automation<float> defaultAutomation = new(World, InterpolationMethod.FloatLinear, [new(0f, 0f)]);
            Automation<float> triangleWave = new(World, InterpolationMethod.FloatLinear, [new(0f, 0f), new(1f, 1f), new(2f, 0f)], true);
            StateMachine machine = new(World);
            machine.AddState("Entry State");
            machine.AddState("Other State");
            machine.AddTransition("Entry State", "Other State", "pastrami", Transition.Condition.GreaterThan, 0f);
            machine.AddTransition("Other State", "Entry State", "pastrami", Transition.Condition.LessThanOrEqual, 0f);
            machine.EntryState = "Entry State";

            Assert.That(machine.EntryState.ToString(), Is.EqualTo("Entry State"));

            Entity entity = new(World);
            entity.AddComponent(0f);
            StatefulAutomationPlayer stateful = entity.Become<StatefulAutomationPlayer>();
            stateful.StateMachine = machine;
            stateful.AddParameter("pastrami", 0f);
            stateful.AddOrSetLink<float>("Entry State", defaultAutomation);
            stateful.AddOrSetLink<float>("Other State", triangleWave);

            Assert.That(stateful.CurrentState.ToString(), Is.EqualTo("Entry State"));

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0f).Within(0.01f));

            stateful.SetParameter("pastrami", 1f);
            Simulator.Update(TimeSpan.FromSeconds(0.1f));
            Simulator.Update(TimeSpan.FromSeconds(0.1f));
            Simulator.Update(TimeSpan.FromSeconds(0.1f));
            Simulator.Update(TimeSpan.FromSeconds(0.1f));
            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0.5f).Within(0.01f));

            Simulator.Update(TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(1f).Within(0.01f));

            Simulator.Update(TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0.5f).Within(0.01f));

            stateful.SetParameter("pastrami", 0f);

            Simulator.Update(TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<float>(), Is.EqualTo(0f).Within(0.01f));
        }
    }
}
