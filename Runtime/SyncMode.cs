// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;

namespace Teo.AutoReference {
    [Flags]
    public enum SyncMode {
        ///<summary>
        /// The default SyncMode which changes depending on the type of the field.
        /// It's equivalent to <see cref="SyncMode.ValidateOrGetIfEmpty"/> for single-value fields and
        /// <see cref="SyncMode.AlwaysGetAndValidate"/> for array or list fields.
        /// </summary>
        Default,

        /// <summary>
        /// Validate values to make sure they fit all constraints, but do not automatically retrieve anything.
        /// </summary>
        ValidateOnly,

        /// <summary>
        /// Only get and validate references if the value is empty
        /// (e.g.null for single-value fields and 0 size for arrays and lists)
        /// </summary>
        GetIfEmpty,

        /// <summary>
        /// Get and validate references if the value is empty (e.g.null for single-value fields and 0 size for
        /// arrays and lists) but also validate any new value that is added or applied if the existing value is not
        /// empty.
        /// </summary>
        ValidateOrGetIfEmpty,

        /// <summary>
        /// Always get and validate references. This means any value the user applies through the inspector will not
        /// be respected and will be overriden the next time the auto-references are synced.
        /// </summary>
        AlwaysGetAndValidate,
    }
}
