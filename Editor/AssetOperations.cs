// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {

    public static class AssetOperations {
        public static readonly string[] AssetsFolder = { "Assets" };

        public static void SyncAllPrefabs() {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);

            var status = SyncStatus.None;

            var guids = AssetDatabase.FindAssets("t:Prefab", AssetsFolder);
            using var behaviours = TempList<MonoBehaviour>.Get();
            using var progress = ProgressBar.Begin("Syncing Auto-References in Prefabs", guids.Length);

            for (var index = 0; index < guids.Length; index++) {
                var guid = guids[index];

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                progress.Update(index, path);

                behaviours.Clear();
                prefab.GetComponentsInChildren(true, behaviours);

                var wereChangesMade = false;

                foreach (var behaviour in behaviours) {
                    status |= AutoReference.Sync(behaviour);
                    wereChangesMade = wereChangesMade || EditorUtility.IsDirty(behaviour);
                }

                if (wereChangesMade) {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }

            LogContext.AppendStatusSummary(status);
        }
    }
}
