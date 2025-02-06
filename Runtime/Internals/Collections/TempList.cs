// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary list that can be disposed and reused.
    /// </summary>
    internal sealed class TempList<T> : List<T>, IDisposable {
        private static readonly Stack<TempList<T>> Pool = new(4);

        private bool _isPooled;

        private TempList() { }

        /// <summary>
        /// Indicates if the list is currently in the pool.
        /// </summary>
        public bool IsPooled => _isPooled;

        /// <summary>
        /// Releases the <c>TempList</c> object back to the pool. If it's already in the pool, it does nothing.
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
        /// Gets a <c>TempList</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempList<T> Get() {
            var list = Pool.TryPop(out var result) ? result : new TempList<T>();
            list.Clear();

            list._isPooled = false;
            return list;
        }

        /// <summary>
        /// Gets a <c>TempList</c> object from the pool or creates a new one if pool is empty, then fills the list
        /// with the elements in the specified collection.
        /// </summary>
        public static TempList<T> Get(IEnumerable<T> enumerable) {
            var list = Pool.TryPop(out var result) ? result : new TempList<T>();
            list.Clear();

            list._isPooled = false;

            list.AddRange(enumerable);
            return list;
        }
    }
}
