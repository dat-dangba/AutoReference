using System;
using System.Collections.Generic;
using System.Text;
using Teo.AutoReference.Configuration;
using UnityEngine;

namespace Teo.AutoReference.Internals {
    internal class LogFormatter : IDisposable {
        private static readonly Stack<LogFormatter> Pool = new();
        private readonly StringBuilder _sb;
        private bool _bold, _italic;
        private Color32? _color;
        private FormatInfo _default;
        private bool _isDisposed;

        private bool _isEnabled;
        private FormatInfo _message;
        private FormatInfo _name;
        private FormatInfo _symbol;

        private LogFormatter() {
            _sb = new StringBuilder();
        }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            _isDisposed = true;
            Clear();
            Pool.Push(this);
        }

        public void AppendText(string text) {
            Append(_default, text);
        }

        public void AppendMessage(string text) {
            Append(_message, text);
        }

        public void AppendExceptionName(string text) {
            Append(_name, text);
        }

        public void AppendSymbol(string text) {
            Append(_symbol, text);
        }

        private void ToggleTag(ref bool current, bool newValue, string open, string close) {
            if (current == newValue) {
                return;
            }
            if (current) {
                _sb.Append(close);
            }
            if (newValue) {
                _sb.Append(open);
            }
            current = newValue;
        }

        private void ToggleColorTag(ref Color32? current, Color32? newValue) {
            if (current.HasValue == newValue.HasValue) {
                if (!current.HasValue) {
                    // Both colors are disabled
                    return;
                }

                var ca = current.Value;
                var cb = newValue.Value;
                if (ca.r == cb.r && ca.g == cb.g && ca.b == cb.b) {
                    // Colors are the same
                    return;
                }
            }

            if (current.HasValue) {
                _sb.Append("</color>");
            }
            if (newValue.HasValue) {
                _sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(newValue.Value)}>");
            }
            current = newValue;
        }

        public void Append(in FormatInfo format, string text) {
            if (!_isEnabled) {
                _sb.Append(text);
                return;
            }

            if (string.IsNullOrEmpty(text)) {
                return;
            }

            if (text.Contains('\n')) {
                // Treat each line as an individual message.
                // i.e. We close the tags at the end of each line and reopen them when necessary.
                // This is necessary because older versions of Unity don't support tags spanning multiple lines.
                var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < lines.Length; ++i) {
                    Append(format, lines[i]);
                    if (i < lines.Length - 1) {
                        AppendLine();
                    }
                }

                if (text.EndsWith('\n')) {
                    AppendLine();
                }
                return;
            }

            var newBold = format.Bold;
            var newItalic = format.Italic;
            var newColor = format.ColorEnabled ? format.Color : (Color32?)null;

            ToggleTag(ref _bold, newBold, "<b>", "</b>");
            ToggleTag(ref _italic, newItalic, "<i>", "</i>");
            ToggleColorTag(ref _color, newColor);

            _sb.Append(text);
        }

        public static LogFormatter Make() {
            if (!Pool.TryPop(out var formatter)) {
                formatter = new LogFormatter();
                formatter.Clear();
            }

            formatter._isDisposed = false;
            formatter.RefreshSettings();
            return formatter;
        }

        public void Clear() {
            _sb.Clear();
            _bold = false;
            _italic = false;
            _color = null;
        }

        private void CloseAllTags() {
            if (_bold) {
                _sb.Append("</b>");
                _bold = false;
            }
            if (_italic) {
                _sb.Append("</i>");
                _italic = false;
            }
            if (_color.HasValue) {
                _sb.Append("</color>");
                _color = null;
            }
        }

        public void AppendLine() {
            CloseAllTags();
            _sb.Append('\n');
        }

        private void RefreshSettings() {
            _isEnabled = SyncPreferences.EnableExceptionFormatting;
            _default = SyncPreferences.DefaultFormatInfo;
            _name = SyncPreferences.ExceptionFormatInfo;
            _message = SyncPreferences.MessageFormatInfo;
            _symbol = SyncPreferences.SymbolFormatInfo;
        }

        public string Build() {
            CloseAllTags();
            return _sb.ToString();
        }
    }
}
