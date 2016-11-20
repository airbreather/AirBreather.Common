using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Collections
{
    public sealed class ReadOnlyListSegment<T> : IReadOnlyList<T>
    {
        public ReadOnlyListSegment(IReadOnlyList<T> list) : this(list, 0, list?.Count ?? 0) { }

        public ReadOnlyListSegment(IReadOnlyList<T> list, int offset, int count)
        {
            this.List = list.ValidateNotNull(nameof(list));
            this.Offset = offset.ValidateInRange(nameof(offset), 0, list.Count);
            this.Count = count.ValidateNotLessThan(nameof(count), 0);

            if (list.Count - offset < count)
            {
                throw new ArgumentException("Segment extends beyond the list bounds.", nameof(list));
            }
        }

        public IReadOnlyList<T> List { get; }
        public int Offset { get; }
        public int Count { get; }

        public T this[int index] => this.List[this.Offset + index.ValidateInRange(nameof(index), 0, this.Count)];

        public IEnumerator<T> GetEnumerator() => this.List.Skip(this.Offset).Take(this.Count).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
