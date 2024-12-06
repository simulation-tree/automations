using Automations.Components;
using Automations.Systems;
using Simulation.Tests;
using System;
using System.Numerics;
using Unmanaged;
using Worlds;

namespace Automations.Tests
{
    public class InterpolationTests : SimulationTests
    {
        protected override void SetUp()
        {
            base.SetUp();
            ComponentType.Register<IsAutomation>();
            ComponentType.Register<IsAutomationPlayer>();
            ComponentType.Register<FixedString>();
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
            Simulator.AddSystem(new AutomationPlayingSystem());
        }

        [Test]
        public void VerifyHold()
        {
            Entity entity = new(World);
            entity.AddComponent<FixedString>();

            Automation<FixedString> animation = new(World);
            animation.AddKeyframe(0f, "this");
            animation.AddKeyframe(1f, "is");
            animation.AddKeyframe(2f, "sum");
            animation.AddKeyframe(3f, "text");

            AutomationPlayer entityPlayer = entity.Become<AutomationPlayer>();
            entityPlayer.SetAutomation<FixedString>(animation);
            entityPlayer.Play();

            Assert.That(animation.Count, Is.EqualTo(4));

            Simulator.Update(TimeSpan.FromSeconds(0f));

            Assert.That(entity.GetComponent<FixedString>().ToString(), Is.EqualTo("this"));

            Simulator.Update(TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<FixedString>().ToString(), Is.EqualTo("this"));

            Simulator.Update(TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<FixedString>().ToString(), Is.EqualTo("is"));

            Simulator.Update(TimeSpan.FromSeconds(1f));

            Assert.That(entity.GetComponent<FixedString>().ToString(), Is.EqualTo("sum"));

            Simulator.Update(TimeSpan.FromSeconds(0.1f));

            Assert.That(entity.GetComponent<FixedString>().ToString(), Is.EqualTo("sum"));

            Simulator.Update(TimeSpan.FromSeconds(0.9f));

            Assert.That(entity.GetComponent<FixedString>().ToString(), Is.EqualTo("text"));

            Simulator.Update(TimeSpan.FromSeconds(0.5f));

            Assert.That(entity.GetComponent<FixedString>().ToString(), Is.EqualTo("text"));
        }

        [Test]
        public void VerifyFloatLinear()
        {
            Interpolation floatInterpolation = BuiltInInterpolations.all[InterpolationMethod.FloatLinear];
            float current = 0f;
            float next = 8f;
            float component = 0f;
            floatInterpolation.Invoke(ref current, ref next, 0f, ref component);
            Assert.That(component, Is.EqualTo(0f).Within(0.01f));

            floatInterpolation.Invoke(ref current, ref next, 0.5f, ref component);
            Assert.That(component, Is.EqualTo(4f).Within(0.01f));

            floatInterpolation.Invoke(ref current, ref next, 1f, ref component);
            Assert.That(component, Is.EqualTo(8f).Within(0.01f));
        }

        [Test]
        public void VerifyVector3Linear()
        {
            Interpolation vector3Interpolation = BuiltInInterpolations.all[InterpolationMethod.Vector3Linear];
            Vector3 current = Vector3.Zero;
            Vector3 next = Vector3.One;
            Vector3 component = Vector3.Zero;

            vector3Interpolation.Invoke(ref current, ref next, 0f, ref component);

            Assert.That(component.X, Is.EqualTo(0f).Within(0.01f));
            Assert.That(component.Y, Is.EqualTo(0f).Within(0.01f));
            Assert.That(component.Z, Is.EqualTo(0f).Within(0.01f));

            vector3Interpolation.Invoke(ref current, ref next, 0.5f, ref component);

            Assert.That(component.X, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(component.Y, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(component.Z, Is.EqualTo(0.5f).Within(0.01f));

            vector3Interpolation.Invoke(ref current, ref next, 1f, ref component);

            Assert.That(component.X, Is.EqualTo(1f).Within(0.01f));
            Assert.That(component.Y, Is.EqualTo(1f).Within(0.01f));
            Assert.That(component.Z, Is.EqualTo(1f).Within(0.01f));
        }

        [Test]
        public unsafe void VerifyInterpolatingFromArray()
        {
            using Allocation component = new(sizeof(float));
            using Allocation keyframes = new(sizeof(float) * 2);
            keyframes.Write(sizeof(float) * 0, 0f);
            keyframes.Write(sizeof(float) * 1, 8f);

            void* current = keyframes.Read(sizeof(float) * 0);
            void* next = keyframes.Read(sizeof(float) * 1);
            Interpolation floatInterpolation = BuiltInInterpolations.all[InterpolationMethod.FloatLinear];
            floatInterpolation.Invoke(current, next, 0.5f, component, sizeof(float));

            Assert.That(component.Read<float>(), Is.EqualTo(4f).Within(0.01f));
        }
    }
}
