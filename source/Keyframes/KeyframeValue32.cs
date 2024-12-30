using System;
using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct KeyframeValue32
    {
        public KeyframeValue16 a;
        public KeyframeValue16 b;

        public KeyframeValue32(KeyframeValue16 a, KeyframeValue16 b)
        {
            this.a = a;
            this.b = b;
        }
    }
}