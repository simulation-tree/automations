using System;
using Worlds;

namespace Automations
{
    [Array]
    public struct KeyframeValue64
    {
        public KeyframeValue32 a;
        public KeyframeValue32 b;

        public KeyframeValue64(KeyframeValue32 a, KeyframeValue32 b)
        {
            this.a = a;
            this.b = b;
        }
    }
}