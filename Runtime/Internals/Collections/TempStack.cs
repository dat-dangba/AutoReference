// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary stack that can be disposed and reused.
    /// </summary>
    internal sealed class TempStack<T> : Stack<T>, IDisposable {
        private static readonly Stack<TempStack<T>> Pool = new(4);

        private bool _isPooled;

        private TempStack() { }

        /// <summary>
        /// Indicates if the stack is currently in the pool.
        /// </summary>
        public bool IsPooled => _isPooled;

        /// <summary>
        /// Releases the <c>TempStack</c> object back to the pool. If it's already in the pool, it does nothing.
        /// </summary>
        public void Dispose() {
            if (_isPooled) {
                return;
            }

            _isPooled = true;

            Clear();
            Pool.Push(this);
        }

        /// <summary>
        /// Gets a <c>TempStack</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempStack<T> Get() {
            var stack = Pool.TryPop(out var result) ? result : new TempStack<T>();
            stack.Clear();

            stack._isPooled = false;
            return stack;
        }
    }
}
