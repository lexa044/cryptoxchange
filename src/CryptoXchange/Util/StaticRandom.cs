using System;
using System.Threading;

namespace CryptoXchange.Util
{
    public static class StaticRandom
    {
        static int seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next()
        {
            return random.Value.Next();
        }

        public static int Next(int n)
        {
            return random.Value.Next(n);
        }
    }
}
