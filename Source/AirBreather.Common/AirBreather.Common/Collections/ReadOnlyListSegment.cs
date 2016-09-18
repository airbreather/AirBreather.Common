using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AirBreather.Collections
{
    public sealed class ReadOnlyListSegment<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> list;
        private readonly int offset;
        private readonly int count;

        public ReadOnlyListSegment(IReadOnlyList<T> list) : this(list, 0, list?.Count ?? 0)
        {
        }

        public ReadOnlyListSegment(IReadOnlyList<T> list, int offset, int count)
        {
            this.list = list.ValidateNotNull(nameof(list));
            this.offset = offset.ValidateInRange(nameof(offset), 0, list.Count);
            this.count = count.ValidateNotLessThan(nameof(count), 0);

            if (list.Count - offset < count)
            {
                throw new ArgumentException("Segment extends beyond the list bounds.", nameof(list));
            }
        }

        public T this[int index] => this.list[this.offset + index.ValidateInRange(nameof(index), 0, this.count)];
        public int Count => this.count;

        public IEnumerator<T> GetEnumerator() => this.list.Skip(this.offset).Take(this.count).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
