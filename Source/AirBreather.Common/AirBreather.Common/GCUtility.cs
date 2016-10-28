using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AirBreather
{
    public static class GCUtility
    {
        public static PinnedHandle Pin(object obj) => new PinnedHandle(GCHandle.Alloc(obj, GCHandleType.Pinned));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestLargeObjectHeapCompaction() => GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

        public static void SeriousCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            RequestLargeObjectHeapCompaction();
            GC.Collect();
        }
    }

    public sealed class PinnedHandle : UnmanagedResourceContainer<GCHandle>
    {
        internal PinnedHandle(GCHandle handle) : base(handle) { }
        protected override void Release(GCHandle resource) => resource.Free();
    }
}
