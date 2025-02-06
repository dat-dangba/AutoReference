// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Teo.AutoReference.Configuration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Teo.AutoReference.Editor {
    [InitializeOnLoad]
    internal static class AutoReferenceEventHook {

        public static void Refresh() {
            EditorSceneManager.sceneSaving -= SyncScene;
            if (SyncPreferences.SyncOnSceneSave) {
                EditorSceneManager.sceneSaving += SyncScene;
            }

            AssemblyReloadEvents.afterAssemblyReload -= SyncOpenScenes;
            if (SyncPreferences.SyncOnAssemblyReload) {
                AssemblyReloadEvents.afterAssemblyReload += SyncOpenScenes;
            }
        }

        static AutoReferenceEventHook() {
            Refresh();
        }

        private static void SyncOpenScenes() {
            SceneOperations.SyncAllOpenScenes();
        }

        private static void SyncScene(Scene scene, string path) {
            SceneOperations.SyncOpenScene(scene);
        }
    }
}
