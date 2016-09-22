using System.Runtime.InteropServices;

namespace AirBreather
{
    public static class IndexTaggedValue
    {
        public static IndexTaggedValue<T> Create<T>(T value, int index) => new IndexTaggedValue<T>(value, index);
    }

    [StructLayout(LayoutKind.Auto)]
    public struct IndexTaggedValue<T>
    {
        internal IndexTaggedValue(T value, int index)
        {
            this.Value = value;
            this.Index = index;
        }

        public T Value { get; }

        public int Index { get; }
    }
}
