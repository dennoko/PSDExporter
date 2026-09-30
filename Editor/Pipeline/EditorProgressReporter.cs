using System;
using UnityEditor;

namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    /// <summary>キャンセル可能なプログレスバー。Dispose でバーを閉じる。</summary>
    public sealed class EditorProgressReporter : IProgressReporter, IDisposable
    {
        private readonly string _title;
        private bool _cancelled;

        public EditorProgressReporter(string title)
        {
            _title = title;
        }

        public bool IsCancelled => _cancelled;

        public void Report(float progress, string message)
        {
            if (EditorUtility.DisplayCancelableProgressBar(_title, message, progress)) _cancelled = true;
        }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
