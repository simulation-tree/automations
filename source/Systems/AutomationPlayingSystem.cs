using Automations.Components;
using Simulation;
using Simulation.Functions;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Unmanaged;

namespace Automations.Systems
{
    public readonly struct AutomationPlayingSystem : ISystem
    {
        private readonly ComponentQuery<IsAutomationPlayer> playerQuery;

        readonly unsafe InitializeFunction ISystem.Initialize => new(&Initialize);
        readonly unsafe IterateFunction ISystem.Iterate => new(&Update);
        readonly unsafe FinalizeFunction ISystem.Finalize => new(&Finalize);

        [UnmanagedCallersOnly]
        private static void Initialize(SystemContainer container, World world)
        {
        }

        [UnmanagedCallersOnly]
        private static void Update(SystemContainer container, World world, TimeSpan delta)
        {
            ref AutomationPlayingSystem system = ref container.Read<AutomationPlayingSystem>();
            system.Update(world, delta);
        }

        [UnmanagedCallersOnly]
        private static void Finalize(SystemContainer container, World world)
        {
            if (container.World == world)
            {
                ref AutomationPlayingSystem system = ref container.Read<AutomationPlayingSystem>();
                system.CleanUp();
            }
        }

        public AutomationPlayingSystem()
        {
            playerQuery = new();
        }

        private void CleanUp()
        {
            playerQuery.Dispose();
        }

        private void Update(World world, TimeSpan delta)
        {
            playerQuery.Update(world);
            foreach (var x in playerQuery)
            {
                uint playerEntity = x.entity;
                ref IsAutomationPlayer player = ref x.Component1;
                if (player.automationReference != default)
                {
                    player.time += delta;
                    uint automationEntity = world.GetReference(playerEntity, player.automationReference);
                    Evaluate(world, playerEntity, player.componentType, automationEntity, player.time);
                }
            }
        }

        private unsafe void Evaluate(World world, uint playerEntity, RuntimeType componentType, uint automationEntity, TimeSpan time)
        {
            IsAutomation automationComponent = world.GetComponent<IsAutomation>(automationEntity);
            RuntimeType keyframeValueType = automationComponent.keyframeType;
            void* keyframes = world.GetArray(automationEntity, keyframeValueType, out uint keyframeCount);
            if (keyframeCount == 0)
            {
                return;
            }

            float timeInSeconds = (float)time.TotalSeconds;
            uint keyframeTypeSize = keyframeValueType.Size;
            float lastKeyframeTime = *(float*)((byte*)keyframes + (keyframeCount - 1) * keyframeTypeSize);
            if (timeInSeconds > lastKeyframeTime)
            {
                if (automationComponent.loop)
                {
                    timeInSeconds %= lastKeyframeTime;
                }
                else
                {
                    timeInSeconds = lastKeyframeTime;
                }
            }

            uint current = uint.MaxValue;
            for (uint i = keyframeCount - 1; i != uint.MaxValue; i--)
            {
                void* keyframe = (byte*)keyframes + i * keyframeTypeSize;
                float keyframeTime = *(float*)keyframe;
                if (keyframeTime < timeInSeconds)
                {
                    current = i;
                    break;
                }
            }

            if (current == uint.MaxValue)
            {
                if (keyframeCount == 1)
                {
                    current = 0;
                }
                else
                {
                    return;
                }
            }

            bool loop = automationComponent.loop;
            uint next = current + 1;
            if (next == keyframeCount)
            {
                if (loop)
                {
                    next = 0;
                }
                else
                {
                    void* firstKeyframe = (byte*)keyframes;
                    if (keyframeValueType == RuntimeType.Get<Keyframe<Vector4>>())
                    {
                        Vector4 keyframeValue = *(Vector4*)((byte*)firstKeyframe + sizeof(float) + sizeof(uint));
                        byte* valueBytes = (byte*)&keyframeValue;
                        world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<Vector4>.ElementSize));
                    }
                    else if (keyframeValueType == RuntimeType.Get<Keyframe<Vector3>>())
                    {
                        Vector3 keyframeValue = *(Vector3*)((byte*)firstKeyframe + sizeof(float) + sizeof(uint));
                        byte* valueBytes = (byte*)&keyframeValue;
                        world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<Vector3>.ElementSize));
                    }
                    else if (keyframeValueType == RuntimeType.Get<Keyframe<Vector2>>())
                    {
                        Vector2 keyframeValue = *(Vector2*)((byte*)firstKeyframe + sizeof(float) + sizeof(uint));
                        byte* valueBytes = (byte*)&keyframeValue;
                        world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<Vector2>.ElementSize));
                    }
                    else if (keyframeValueType == RuntimeType.Get<Keyframe<float>>())
                    {
                        float keyframeValue = *(float*)((byte*)firstKeyframe + sizeof(float) + sizeof(uint));
                        byte* valueBytes = (byte*)&keyframeValue;
                        world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<float>.ElementSize));
                    }
                    else if (keyframeValueType == RuntimeType.Get<Keyframe<FixedString>>())
                    {
                        FixedString keyframeValue = *(FixedString*)((byte*)firstKeyframe + sizeof(float) + sizeof(uint));
                        byte* valueBytes = (byte*)&keyframeValue;
                        world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<FixedString>.ElementSize));
                    }
                    else
                    {
                        throw new NotImplementedException($"Unable to evaluate with unknown keyframe value type {keyframeValueType} on entity {automationEntity}");
                    }

                    return;
                }
            }

            void* currentKeyframe = (byte*)keyframes + current * keyframeTypeSize;
            void* nextKeyframe = (byte*)keyframes + next * keyframeTypeSize;
            float currentKeyframeTime = *(float*)currentKeyframe;
            float nextKeyframeTime = *(float*)nextKeyframe;
            float timeDelta = nextKeyframeTime - currentKeyframeTime;
            float timeProgress = (timeInSeconds - currentKeyframeTime) / timeDelta;
            uint byteOffset = *(uint*)((byte*)currentKeyframe + sizeof(float)); //todo: handle byte offsets
            if (keyframeValueType == RuntimeType.Get<Keyframe<Vector4>>())
            {
                Vector4 currentKeyframeValue = *(Vector4*)((byte*)currentKeyframe + sizeof(float) + sizeof(uint));
                Vector4 nextKeyframeValue = *(Vector4*)((byte*)nextKeyframe + sizeof(float) + sizeof(uint));
                Vector4 value = Vector4.Lerp(currentKeyframeValue, nextKeyframeValue, timeProgress);
                byte* valueBytes = (byte*)&value;
                world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<Vector4>.ElementSize));
            }
            else if (keyframeValueType == RuntimeType.Get<Keyframe<Vector3>>())
            {
                Vector3 currentKeyframeValue = *(Vector3*)((byte*)currentKeyframe + sizeof(float) + sizeof(uint));
                Vector3 nextKeyframeValue = *(Vector3*)((byte*)nextKeyframe + sizeof(float) + sizeof(uint));
                Vector3 value = Vector3.Lerp(currentKeyframeValue, nextKeyframeValue, timeProgress);
                byte* valueBytes = (byte*)&value;
                world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<Vector3>.ElementSize));
            }
            else if (keyframeValueType == RuntimeType.Get<Keyframe<Vector2>>())
            {
                Vector2 currentKeyframeValue = *(Vector2*)((byte*)currentKeyframe + sizeof(float) + sizeof(uint));
                Vector2 nextKeyframeValue = *(Vector2*)((byte*)nextKeyframe + sizeof(float) + sizeof(uint));
                Vector2 value = Vector2.Lerp(currentKeyframeValue, nextKeyframeValue, timeProgress);
                byte* valueBytes = (byte*)&value;
                world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<Vector2>.ElementSize));
            }
            else if (keyframeValueType == RuntimeType.Get<Keyframe<float>>())
            {
                float currentKeyframeValue = *(float*)((byte*)currentKeyframe + sizeof(float) + sizeof(uint));
                float nextKeyframeValue = *(float*)((byte*)nextKeyframe + sizeof(float) + sizeof(uint));
                float value = currentKeyframeValue + (nextKeyframeValue - currentKeyframeValue) * timeProgress;
                byte* valueBytes = (byte*)&value;
                world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<float>.ElementSize));
            }
            else if (keyframeValueType == RuntimeType.Get<byte>())
            {
                byte currentKeyframeValue = *(byte*)((byte*)currentKeyframe + sizeof(float) + sizeof(uint));
                byte nextKeyframeValue = *(byte*)((byte*)nextKeyframe + sizeof(float) + sizeof(uint));
                byte value = (byte)(currentKeyframeValue + (nextKeyframeValue - currentKeyframeValue) * timeProgress);
                byte* valueBytes = &value;
                world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<byte>.ElementSize));
            }
            else if (keyframeValueType == RuntimeType.Get<Keyframe<FixedString>>())
            {
                FixedString currentKeyframeValue = *(FixedString*)((byte*)currentKeyframe + sizeof(float) + sizeof(uint));
                //FixedString nextKeyframeValue = *(FixedString*)((byte*)nextKeyframe + sizeof(float) + sizeof(uint));
                //FixedString value = FixedString.Lerp(currentKeyframeValue, nextKeyframeValue, timeProgress);
                byte* valueBytes = (byte*)&currentKeyframeValue;
                world.SetComponent(playerEntity, componentType, new USpan<byte>(valueBytes, USpan<FixedString>.ElementSize));
            }
            else
            {
                throw new NotImplementedException($"Unable to evaluate with unknown keyframe value type {keyframeValueType} on entity {automationEntity}");
            }
        }
    }
}