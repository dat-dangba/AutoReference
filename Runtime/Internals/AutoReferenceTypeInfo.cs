// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System.Reflection;

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Contains all auto-reference information of a type containing auto-reference fields or after-sync
    /// callbacks.
    /// </summary>
    internal struct AutoReferenceTypeInfo {
        public AutoReferenceField[] autoReferenceFields;
        public MethodInfo[] syncCallbacks;
        public FieldInfo[] syncedFields;
        public LogItem[] messages;

        public int declaredCallbacksCount;

        public readonly bool IsSyncable => autoReferenceFields.Length + syncCallbacks.Length > 0;

        public readonly bool HasSyncableFields => autoReferenceFields.Length + syncedFields.Length > 0;
    }
}
