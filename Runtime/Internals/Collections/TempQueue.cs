// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary queue that can be disposed and reused.
    /// </summary>
    internal sealed class TempQueue<T> : Queue<T>, IDisposable {
        private static readonly Stack<TempQueue<T>> Pool = new(4);

        private bool _isPooled;

        private TempQueue() { }

        /// <summary>
        /// Indicates if the queue is currently in the pool.
        /// </summary>
        public bool IsPooled => _isPooled;

        /// <summary>
        /// Releases the <c>TempQueue</c> object back to the pool. If it's already in the pool, it does nothing.
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
        /// Gets a <c>TempQueue</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempQueue<T> Get() {
            var queue = Pool.TryPop(out var result) ? result : new TempQueue<T>();
            queue.Clear();

            queue._isPooled = false;
            return queue;
        }
    }
}
