// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary list that can be disposed and reused.
    /// </summary>
    internal sealed class TempSet<T> : HashSet<T>, IDisposable {
        private static readonly Stack<TempSet<T>> Pool = new(4);

        private bool _isPooled;

        private TempSet() { }

        /// <summary>
        /// Indicates if the list is currently in the pool.
        /// </summary>
        public bool IsPooled => _isPooled;

        /// <summary>
        /// Releases the <c>TempHashSet</c> object back to the pool. If it's already in the pool, it does nothing.
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
        /// Gets a <c>TempHashSet</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempSet<T> Get() {
            var list = Pool.TryPop(out var result) ? result : new TempSet<T>();
            list.Clear();

            list._isPooled = false;
            return list;
        }

        /// <summary>
        /// Gets a <c>TempHashSet</c> object from the pool or creates a new one if pool is empty, then fills the list
        /// with the elements in the specified collection.
        /// </summary>
        public static TempSet<T> Get(IEnumerable<T> enumerable) {
            var set = Pool.TryPop(out var result) ? result : new TempSet<T>();
            set.Clear();

            set._isPooled = false;

            set.UnionWith(enumerable);
            return set;
        }
    }
}
