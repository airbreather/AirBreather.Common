using System;
using System.Collections.Generic;

namespace AirBreather.Collections
{
    public sealed class CallbackComparer<T> : Comparer<T>
    {
        private readonly Func<T, T, int> callback;

        public CallbackComparer(Func<T, T, int> callback) => this.callback = callback.ValidateNotNull(nameof(callback));

        public override int Compare(T x, T y) => this.callback(x, y);
    }
}
