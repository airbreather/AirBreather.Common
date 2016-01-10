using System;
using System.Collections;
using System.Collections.Generic;

namespace AirBreather.Common.Utilities
{
    public sealed class LazyBufferedEnumerable<T> : DisposableObject, IEnumerable<T>
    {
        private readonly List<T> buffer;
        private readonly IEnumerator<T> enumerator;
        private readonly object syncLock = new object();

        private bool enumerationFinished;

        public LazyBufferedEnumerable(IEnumerable<T> enumerable)
        {
            int? cnt = enumerable.ValidateNotNull(nameof(enumerable)).GetCountPropertyIfAvailable();
            this.enumerator = enumerable.GetEnumerator();
            this.buffer = cnt.HasValue
                ? new List<T>(cnt.Value)
                : new List<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            this.ThrowIfDisposed();
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        protected override void DisposeCore() => this.enumerator.Dispose();

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
                    return false;
                }

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

                    this.buffered.enumerationFinished = true;
                    return false;
                }
            }

            public void Reset() => this.idx = -1;

            void IDisposable.Dispose()
            {
            }
        }
    }
}
