using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct KeyframeTime
    {
        public float time;

        public KeyframeTime(float time)
        {
            this.time = time;
        }

        public static implicit operator float(KeyframeTime keyframeTime) => keyframeTime.time;
        public static implicit operator KeyframeTime(float time) => new(time);
    }
}