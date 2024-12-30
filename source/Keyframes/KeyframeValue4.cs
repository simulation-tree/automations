using System;
using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct KeyframeValue4
    {
        public KeyframeValue2 a;
        public KeyframeValue2 b;

        public KeyframeValue4(KeyframeValue2 a, KeyframeValue2 b)
        {
            this.a = a;
            this.b = b;
        }
    }
}