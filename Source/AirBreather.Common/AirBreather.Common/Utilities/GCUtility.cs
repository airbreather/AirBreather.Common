using System.Runtime.InteropServices;

namespace AirBreather.Common.Utilities
{
    public static class GCUtility
    {
        public static PinnedHandle Pin(object obj) => new PinnedHandle(GCHandle.Alloc(obj, GCHandleType.Pinned));
    }

    public sealed class PinnedHandle : UnmanagedResourceContainer<GCHandle>
    {
        internal PinnedHandle(GCHandle handle) : base(handle) { }
        protected override void Release(GCHandle resource) => resource.Free();
    }
}
