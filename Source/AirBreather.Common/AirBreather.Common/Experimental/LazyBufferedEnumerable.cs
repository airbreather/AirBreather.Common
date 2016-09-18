using System;
using System.Collections;
using System.Collections.Generic;

namespace AirBreather
{
    public sealed class LazyBufferedEnumerable<T> : DisposableObject, IEnumerable<T>
    {
        private readonly List<T> buffer;
        private readonly object syncLock = new object();

        private IEnumerator<T> enumerator;
        private bool enumerationFinished;

        public LazyBufferedEnumerable(IEnumerable<T> enumerable)
        {
            this.enumerator = enumerable.ValidateNotNull(nameof(enumerable)).GetEnumerator();

            int cnt;
            this.buffer = enumerable.TryGetCount(out cnt)
                ? new List<T>(cnt)
                : new List<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            this.ThrowIfDisposed();
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        protected override void DisposeCore() => this.enumerator?.Dispose();

        private sealed class Enumerator : IEnumerator<T>
        {
            private readonly LazyBufferedEnumerable<T> buffered;
            private int idx = -1;

            internal Enumerator(LazyBufferedEnumerable<T> buffered)
            {
                this.buffered = buffered;
            }

            public T Current => this.buffered.buffer[this.idx];
            object IEnumerator.Current => this.Current;

            public bool MoveNext()
            {
                if (++this.idx < this.buffered.buffer.Count)
                {
                    return true;
                }

                if (this.buffered.enumerationFinished)
                {
                    --this.idx;
                    return false;
                }

                // because this synchronization is necessary, try to avoid using this except when
                // it's particularly likely to be needed... it's actually highly unlikely that we'll
                // ever find a situation where this is the right thing.
                lock (this.buffered.syncLock)
                {
                    if (this.idx < this.buffered.buffer.Count)
                    {
                        return true;
                    }

                    if (this.buffered.enumerationFinished)
                    {
                        return false;
                    }

                    if (this.buffered.enumerator.MoveNext())
                    {
                        this.buffered.buffer.Add(this.buffered.enumerator.Current);
                        return true;
                    }

                    using (this.buffered.enumerator)
                    {
                        this.buffered.enumerator = null;
                        this.buffered.enumerationFinished = true;
                        return false;
                    }
                }
            }

            public void Reset() => this.idx = -1;

            void IDisposable.Dispose()
            {
            }
        }
    }
}
