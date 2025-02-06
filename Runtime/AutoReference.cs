// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Provides methods for syncing and processing Auto-References in <see cref="MonoBehaviour"/> scripts.
    /// </summary>
    public static class AutoReference {
        private static readonly Dictionary<Type, AutoReferenceTypeInfo> CachedFieldData = new();

        // We're using a list instead of a hash map because the size of elements is always going to be very small.
        // List is always faster for small sizes than a hash map.
        // Note: tracking syncing references used to be required but not anymore, but it still remains as a feature.
        private static readonly List<MonoBehaviour> SyncingReferences = new(10);

        private static readonly ObjectWatcher Watcher = new();

        static AutoReference() {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += () => {
                CachedFieldData.Clear();
                SyncingReferences.Clear();
            };
#endif
        }

        public static int CacheCount => CachedFieldData.Count;


        /// <summary>
        /// Syncs all auto-references of all <see cref="MonoBehaviour"/>s on the given <see cref="GameObject"/>.
        /// </summary>
        public static SyncStatus Sync(GameObject gameObject) {
            if (Application.isPlaying) {
                return SyncStatus.None;
            }

            using var behaviours = TempList<MonoBehaviour>.Get();
            gameObject.GetComponents(behaviours);

            var status = behaviours.Aggregate(
                SyncStatus.None, (current, behaviour) => current | SyncNoAppend(behaviour)
            );

            LogContext.AppendStatusSummary(status);

            return status;
        }

        private static SyncStatus SyncNoAppend(MonoBehaviour behaviour) {
            if (Application.isPlaying) {
                return SyncStatus.Unsupported;
            }

            if (behaviour == null || SyncingReferences.Contains(behaviour)) {
                return SyncStatus.Skip;
            }

            var metadata = GetAutoReferenceInfo(behaviour.GetType());

            if (DoSync(behaviour, metadata, out var syncStatus)) {
                SetDirty(behaviour);
            }

            return syncStatus | SyncStatus.Complete;
        }

        /// <summary>
        /// Syncs all auto-references on the given <see cref="MonoBehaviour"/>.
        /// </summary>
        public static SyncStatus Sync(MonoBehaviour behaviour) {
            var status = SyncNoAppend(behaviour);
            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Check that the the provided <see cref="MonoBehaviour"/> has just finished syncing and is currently
        /// validating through the <see cref="OnAfterSyncAttribute"/> method calls.
        /// </summary>
        /// <param name="behaviour">The <see cref="MonoBehaviour"/> that is being verified.</param>
        public static bool IsAfterSyncValidating(MonoBehaviour behaviour) {
            return SyncingReferences.Contains(behaviour);
        }

        internal static AutoReferenceTypeInfo GetAutoReferenceInfo(Type type) {
            // Attempt to get reflection info from the cache. This info contains an auto-reference attribute,
            // all auto-reference filters, and references to after-sync callback methods.

            if (!SyncPreferences.CacheSyncInfo) {
                return AutoReferenceResolver.GetAutoReferenceInfo(type);
            }

            if (CachedFieldData.TryGetValue(type, out var info)) {
                return info;
            }

            return CachedFieldData[type] = AutoReferenceResolver.GetAutoReferenceInfo(type);
        }


        public static bool IsInScene(Object obj) {
            return obj.GetPrefabMode() == ObjectUtils.EditingMode.InScene;
        }

        public static bool IsPrefab(Object obj) {
            return obj.GetPrefabMode() == ObjectUtils.EditingMode.InPrefab;
        }

        /// <summary>
        /// This method performs auto-reference syncing on a MonoBehaviour from the sync information taken from its
        /// type. It also runs after-sync validation calls and checks and returns if any synced fields have been
        /// modified.
        /// </summary>
        private static bool DoSync(
            MonoBehaviour behaviour,
            in AutoReferenceTypeInfo info,
            out SyncStatus status
        ) {
            status = LogItem.ProcessMessages(info.messages);

            if (!info.IsSyncable) {
                status |= SyncStatus.Skip;
                return false;
            }

            // If no synced fields exist (which include auto-reference fields and fields marked with Sync attribute)
            // then no change detection is necessary, we just run the callbacks and assume nothing changed.
            if (!info.HasSyncableFields) {
                RunAfterSyncCallbacks(behaviour, info.syncCallbacks);
                return false;
            }

            // The watcher creates an internal serializable object that it uses to detect changes.
            using var watcher = Watcher.Init(behaviour, info);

            foreach (var autoField in info.autoReferenceFields) {
                status |= autoField.MainAttribute.SyncReferences(behaviour);
            }

            RunAfterSyncCallbacks(behaviour, info.syncCallbacks);

            return watcher.IsObjectModified();
        }

        private static void LogSyncException(Exception exception, MethodInfo callback) {
            var message = Formatter.FormaMethodException(exception, callback, "Sync method ");
            Debug.LogError(message);
        }

        private static void RunAfterSyncCallbacks(MonoBehaviour behaviour, MethodInfo[] callbacks) {
            foreach (var callback in callbacks) {
                try {
                    callback.Invoke(behaviour, null);
                } catch (Exception e) {
                    LogSyncException(e, callback);
                }
            }
        }

        public static bool HasSyncInformation(MonoBehaviour behaviour) {
            return behaviour != null && GetAutoReferenceInfo(behaviour.GetType()).IsSyncable;
        }

        public static bool HasSyncInformation(Type type) {
            return type.IsSubclassOf(Types.Mono) && GetAutoReferenceInfo(type).IsSyncable;
        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDirty(Object target) {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
#endif
        }

        public static void ClearCache() {
            var typeCount = CachedFieldData.Count;

            if (typeCount == 0) {
                return;
            }

            CachedFieldData.Clear();
            var types = Formatter.FormatCount(typeCount, "type");
            Debug.Log($"Cleared cached Auto-Reference information of {types}.");
        }
    }
}
