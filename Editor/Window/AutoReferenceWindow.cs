// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor.Window {
    public class AutoReferenceWindow : EditorWindow {
        private const string PreferencePath = "Teo.AutoReference.Editor.Window";
        [SerializeField] private StateInfo state;

        private bool _isInitialized;
        private AutoReferenceWindowContent _treeView;

        private void OnEnable() {
            if (_treeView != null) {
                _treeView.Reload();
                return;
            }

            if (!state.IsValid) {
                ReadFromPrefs(ref state);
            }

            _treeView = AutoReferenceWindowContent.Create(ref state);

            EditorApplication.delayCall += () => { _treeView.RefreshReports(); };
        }

        private void OnDestroy() {
            if (state.IsValid) {
                WriteToPrefs(state);
            }
        }

        private void OnGUI() {
            var rect = new Rect(0, 0, position.width, position.height);
            _treeView.OnGUI(rect);
        }

        public static void Open() {
            var window = GetWindow<AutoReferenceWindow>("Auto-Reference");
            window.titleContent = AutoReferenceWindowContent.CreateTitleContent();
            window.Show();
        }

        public static void WriteToPrefs(in StateInfo data) {
            var json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(PreferencePath, json);
        }

        private static void ReadFromPrefs(ref StateInfo data) {
            var json = EditorPrefs.GetString(PreferencePath, string.Empty);
            if (json != string.Empty) {
                try {
                    data = JsonUtility.FromJson<StateInfo>(json);
                    return;
                } catch (ArgumentException) {
                    // Ignore
                }
            }

            data = default;
        }
    }
}
