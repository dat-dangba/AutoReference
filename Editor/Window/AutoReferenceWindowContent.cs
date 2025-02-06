// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference.Editor.Window {
    internal partial class AutoReferenceWindowContent : TreeView {
        private const int StatusColumn = 0;
        private const int NamespaceColumn = 1;
        private const int TypeColumn = 2;
        private const int TargetColumn = 3;
        private const int AttributeColumn = 4;
        private const int DescriptionColumn = 5;

        private static readonly Texture ErrorIcon = GetIcon("console.erroricon.sml");
        private static readonly Texture WarningIcon = GetIcon("console.warnicon.sml");
        private static readonly Texture ErrorIconInactive = GetIcon("console.erroricon.inactive.sml");
        private static readonly Texture WarningIconInactive = GetIcon("console.warnicon.inactive.sml");
        private static readonly Texture Icon = GetIcon("FilterByType");

        private static readonly MultiColumnHeaderState.Column[] Columns = Columns = new[] {
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent(""), width = 24, minWidth = 24, maxWidth = 24,
                allowToggleVisibility = false, autoResize = false,
                contextMenuText = "Status",
            },
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Namespace"), width = 80, minWidth = 50, autoResize = false,
            },
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Type"), width = 80, minWidth = 50, autoResize = false,
            },
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Target"), width = 80, minWidth = 50, autoResize = false,
            },
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Attribute"), width = 80, minWidth = 60, autoResize = false,
            },
            new MultiColumnHeaderState.Column {
                headerContent = new GUIContent("Description"), width = 200, minWidth = 200, autoResize = true,
                allowToggleVisibility = false,
            },
        };

        private ColumnComparer _columnComparer;
        private TreeViewItem[] _defaultRows;

        private ReportInfo[] _reports;
        private StatisticsInfo _stats;

        private AutoReferenceWindowContent(TreeViewState state, MultiColumnHeader header) : base(state, header) {
            showAlternatingRowBackgrounds = true;
            cellMargin = 2;
            showBorder = true;

            Reload();

            header.sortingChanged += h => SortRows(h, GetRows() as List<TreeViewItem>);
        }

        public static GUIContent CreateTitleContent() {
            return new GUIContent("Auto-Reference", Icon);
        }

        private void SortRows(MultiColumnHeader header, List<TreeViewItem> rows) {
            if (header.sortedColumnIndex == -1) {
                rows.Clear();
                rows.AddRange(_defaultRows);
                return;
            }

            _columnComparer ??= new ColumnComparer(header);

            // List<T>.Sort doesn't use stable sort, so we use OrderBy to sort rows instead.
            var tempList = rows.OrderBy(row => row, _columnComparer).ToTempList();
            rows.Clear();
            rows.AddRange(tempList);
        }

        protected override TreeViewItem BuildRoot() {
            return new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
            if (_reports is not { Length: not 0 }) {
                return new List<TreeViewItem>();
            }

            var id = 0;
            _defaultRows = _reports
                .SelectMany(report => report.items.Select(logItem => new LogItemRow(id++, report.script, logItem)))
                .Cast<TreeViewItem>()
                .ToArray();

            var list = new List<TreeViewItem>(_defaultRows);
            if (multiColumnHeader.sortedColumnIndex != -1) {
                SortRows(multiColumnHeader, list);
            }

            return list;
        }

        public void RefreshReports() {
            (_reports, _stats) = GetReports();
            InitializeGUIOnReport();
            Reload();
        }

        protected override void DoubleClickedItem(int id) {
            var rows = GetRows();
            if (rows[id] is LogItemRow row) {
                AssetDatabase.OpenAsset(row.script);
            }
        }

        public static AutoReferenceWindowContent Create(ref StateInfo state) {
            if (!state.IsValid) {
                state = new StateInfo(new TreeViewState(), new MultiColumnHeaderState(Columns));
            }

            var header = new Header(state.HeaderState);
            return new AutoReferenceWindowContent(state.TreeViewState, header);
        }

        private static MonoScript[] GetMonoTypes() {
            var allScriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            using var results = TempList<MonoScript>.Get();

            results.AddRange(allScriptPaths.Select(scriptPath => "Assets" + scriptPath[Application.dataPath.Length..])
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                .Where(monoScript => monoScript != null)
                .Where(script => script.GetClass()?.IsSubclassOf(Types.Mono) is true));

            return results.ToArray();
        }

        private static Texture GetIcon(string icon) {
            return EditorGUIUtility.IconContent(icon).image;
        }

        protected override void ContextClickedItem(int id) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open File"), false, () => DoubleClickedItem(id));
            menu.ShowAsContext();
        }

        private class ColumnComparer : IComparer<TreeViewItem> {
            private readonly MultiColumnHeader _header;

            public ColumnComparer(MultiColumnHeader header) {
                _header = header;
            }

            private int Column => _header.sortedColumnIndex;

            public int Compare(TreeViewItem x, TreeViewItem y) {
                var ascending = _header.IsSortedAscending(Column);

                if (x is not LogItemRow lhs || y is not LogItemRow rhs) {
                    return 0;
                }

                if (!ascending) {
                    // Swap operands for descending order.
                    (lhs, rhs) = (rhs, lhs);
                }

                return string.Compare(lhs.GetContent(Column), rhs.GetContent(Column), StringComparison.Ordinal);
            }
        }

        private class LogItemRow : TreeViewItem {
            private readonly LogItem _item;
            public readonly MonoScript script;

            public LogItemRow(int id, MonoScript script, LogItem item) : base(id, -1) {
                this.script = script;
                _item = item;
            }

            public string Description => _item.Message;

            public string GetContent(int column) {
                return column switch {
                    StatusColumn => _item.IsError ? "Error" : "Warning",
                    NamespaceColumn => _item.DeclaringType.Namespace,
                    TypeColumn => _item.DeclaringType.FormatCSharpName(),
                    TargetColumn => _item.MemberName,
                    AttributeColumn => _item.AttributeName,
                    DescriptionColumn => _item.Message,
                    _ => "",
                };
            }

            public void DrawContent(AutoReferenceWindowContent content, Rect rect, int column) {
                if (column == 0) {
                    GUI.Label(rect, _item.IsError ? ErrorIcon : WarningIcon, content._centeredLabelStyle);
                } else {
                    GUI.Label(rect, GetContent(column), content._labelStyle);
                }
            }
        }

        private class Header : MultiColumnHeader {
            public Header(MultiColumnHeaderState state) : base(state) { }

            protected override void AddColumnHeaderContextMenuItems(GenericMenu menu) {
                using var visibility = TempList<bool>.Get(
                    Enumerable.Range(0, state.columns.Length).Select(_ => false)
                );
                foreach (var column in state.visibleColumns) {
                    visibility[column] = true;
                }

                for (var i = 0; i < state.columns.Length; i++) {
                    var index = i;
                    if (!state.columns[index].allowToggleVisibility) {
                        continue;
                    }

                    menu.AddItem(
                        new GUIContent($"{state.columns[index].headerContent.text}"),
                        visibility[index],
                        () => ToggleVisibility(index)
                    );
                }

                menu.AddSeparator("");
                menu.AddItem(
                    new GUIContent("Clear Sorting"),
                    false, () => { sortedColumnIndex = -1; }
                );
            }
        }
    }
}
