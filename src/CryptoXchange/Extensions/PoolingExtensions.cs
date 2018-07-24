using System.Collections.Generic;
using CryptoXchange.Buffers;

namespace CryptoXchange.Extensions
{
    public static class PoolingExtensions
    {
        public static void Dispose<T>(this IEnumerable<PooledArraySegment<T>> col)
        {
            foreach (var seg in col)
                seg.Dispose();
        }
    }
}
