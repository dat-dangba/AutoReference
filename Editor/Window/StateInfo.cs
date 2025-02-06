// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Teo.AutoReference.Editor.Window {
    [Serializable]
    public struct StateInfo {
        [SerializeField] private TreeViewState treeViewState;
        [SerializeField] private MultiColumnHeaderState headerState;

        public StateInfo(TreeViewState treeViewState, MultiColumnHeaderState headerState) {
            this.treeViewState = treeViewState;
            this.headerState = headerState;
        }

        public TreeViewState TreeViewState => treeViewState;
        public MultiColumnHeaderState HeaderState => headerState;

        public bool IsValid => TreeViewState != null && HeaderState != null && HeaderState.columns.Length > 0;
    }
}
