using System;
using System.Collections.Generic;

using AirBreather.Common.Caching;

using Xunit;
using Xunit.Abstractions;

namespace AirBreather.Common.Tests
{
    public sealed class MemoryCachePlusTests
    {
        private readonly ITestOutputHelper output;

        public MemoryCachePlusTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // proof-of-concept stuff, really.
        [Fact]
        public void PrintHeapTest()
        {
            var memCache = BuildCache();
            this.output.WriteLine(memCache.PrintHeap());
            GC.KeepAlive(memCache);
        }

        private static MemoryCachePlus BuildCache()
        {
            object shared12_16 = new object();
            object shared13 = new object();
            object shared14 = new object();
            List<object> sharedListOfUnsharedObjects15 = new List<object> { new object() };

            var memCache = new MemoryCachePlus();

            // boxes for [0, 10] are all shared by design
            // box for 11 is created on-the-fly, so it will be unshared.
            memCache.Put(Boxes.Int32(0));
            memCache.Put(Boxes.Int32(1));
            memCache.Put(Boxes.Int32(2));
            memCache.Put(Boxes.Int32(3));
            memCache.Put(Boxes.Int32(4));
            memCache.Put(Boxes.Int32(5));
            memCache.Put(Boxes.Int32(6));
            memCache.Put(Boxes.Int32(7));
            memCache.Put(Boxes.Int32(8));
            memCache.Put(Boxes.Int32(9));
            memCache.Put(Boxes.Int32(10));
            memCache.Put(Boxes.Int32(11));

            memCache.Put(shared12_16);
            memCache.Put(shared13);
            memCache.Put(shared14);
            memCache.Put(sharedListOfUnsharedObjects15);

            // shared because it's the same as index 12... we would have to remove both
            // appearances from the list for it to become eligible.
            memCache.Put(shared12_16);

            // Everything after 16 is unshared
            memCache.Put(new object());
            memCache.Put(new object());
            memCache.Put(new List<object> { new object() });

            // a list that contains itself... spooky...
            List<object> unsharedListOfSharedObjects = new List<object> { shared13, shared14, sharedListOfUnsharedObjects15 };
            unsharedListOfSharedObjects.Add(unsharedListOfSharedObjects);
            memCache.Put(unsharedListOfSharedObjects);

            return memCache;
        }
    }
}
