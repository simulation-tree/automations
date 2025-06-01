using System.Numerics;
using System.Runtime.InteropServices;

namespace Automations
{
    public static class BuiltInInterpolations
    {
        public unsafe static readonly Interpolation[] all =
        [
            new(&FloatLinear),
            new(&Vector2Linear),
            new(&Vector3Linear),
            new(&Vector4Linear),
        ];

        [UnmanagedCallersOnly]
        private static void FloatLinear(Interpolation.Input input)
        {
            float current = input.GetCurrent<float>();
            float next = input.GetNext<float>();
            input.Set(float.Lerp(current, next, input.progress));
        }

        [UnmanagedCallersOnly]
        private static void Vector2Linear(Interpolation.Input input)
        {
            Vector2 current = input.GetCurrent<Vector2>();
            Vector2 next = input.GetNext<Vector2>();
            input.Set(Vector2.Lerp(current, next, input.progress));
        }

        [UnmanagedCallersOnly]
        private static void Vector3Linear(Interpolation.Input input)
        {
            Vector3 current = input.GetCurrent<Vector3>();
            Vector3 next = input.GetNext<Vector3>();
            input.Set(Vector3.Lerp(current, next, input.progress));
        }

        [UnmanagedCallersOnly]
        private static void Vector4Linear(Interpolation.Input input)
        {
            Vector4 current = input.GetCurrent<Vector4>();
            Vector4 next = input.GetNext<Vector4>();
            input.Set(Vector4.Lerp(current, next, input.progress));
        }
    }
}