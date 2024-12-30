using System;
using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct KeyframeValue16
    {
        public KeyframeValue8 a;
        public KeyframeValue8 b;

        public KeyframeValue16(KeyframeValue8 a, KeyframeValue8 b)
        {
            this.a = a;
            this.b = b;
        }
    }
}