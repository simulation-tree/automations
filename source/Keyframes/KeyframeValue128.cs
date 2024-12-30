using System;
using Worlds;

namespace Automations
{
    [ArrayElement]
    public struct KeyframeValue128
    {
        public KeyframeValue64 a;
        public KeyframeValue64 b;
        public KeyframeValue128(KeyframeValue64 a, KeyframeValue64 b)
        {
            this.a = a;
            this.b = b;
        }
    }
}