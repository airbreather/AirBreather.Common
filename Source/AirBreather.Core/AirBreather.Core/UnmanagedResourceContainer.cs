using System;

namespace AirBreather.Core
{
    // my own IDisposable pattern, part 2 of 2
    // nothing IDisposable will directly own unmanaged resources
    // except subclasses of this, and that's the ONLY thing they do.
    // things that would otherwise directly own an unmanaged resource
    // will instead own something that implements this, which is managed.
    //
    // I kinda wish there was a reasonable way to enforce that the
    // ONLY things that the subclass does are override Release() and
    // provide a public constructor that just passes the resource
    // to our base constructor after possibly validating it.
    //
    // The only thread-safety concern to be aware of: make sure to dispose
    // only after all threads are known to have finished consuming it.
    public abstract class UnmanagedResourceContainer<T> : IDisposable
    {
        private readonly T resource;

        protected UnmanagedResourceContainer(T resource)
        {
            this.resource = resource;
        }

        ~UnmanagedResourceContainer()
        {
            // this won't happen if we get disposed properly,
            // because of the SuppressFinalize call,
            // so we don't check IsDisposed.
            this.Release(this.resource);
        }

        public T Resource
        {
            get
            {
                this.ThrowIfDisposed();
                return this.resource;
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.Release(this.resource);
            this.IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        protected abstract void Release(T resource);

        private void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}
