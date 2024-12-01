using System;
using Worlds;

namespace Automations
{
    [Array]
    public struct KeyframeValue256
    {
        public KeyframeValue128 a;
        public KeyframeValue128 b;

        public KeyframeValue256(KeyframeValue128 a, KeyframeValue128 b)
        {
            this.a = a;
            this.b = b;
        }
    }
}