using System;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading;

namespace AirBreather.Common.Caching
{
    public sealed class BufferCache : DisposableObject
    {
        private static long cacheInstance = -1;
        private static readonly CacheItemPolicy DefaultCacheItemPolicy = new CacheItemPolicy();
        private readonly MemoryCache underlyingCache = new MemoryCache("BufferCache" + Interlocked.Increment(ref cacheInstance).ToString(CultureInfo.InvariantCulture));

        public void Put(Guid key, byte[] buffer) => this.underlyingCache.Set(key.ToString("N"), buffer, DefaultCacheItemPolicy);
        public bool TryGet(Guid key, out byte[] buffer) => (buffer = (byte[])this.underlyingCache.Get(key.ToString("N"))) != null;
        protected override void DisposeCore() => this.underlyingCache.Dispose();
    }
}
