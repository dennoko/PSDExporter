using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Composition;

namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    /// <summary>Core の合成進捗を、スロット単位の範囲に割り当ててプログレスバーへ中継する。</summary>
    internal sealed class ProgressCompositionObserver : ICompositionObserver
    {
        private readonly IProgressReporter _progress;
        private readonly float _start;
        private readonly float _span;
        private readonly string _label;
        private readonly List<string> _warnings;

        public ProgressCompositionObserver(IProgressReporter progress, float start, float span, string label, List<string> warnings)
        {
            _progress = progress;
            _start = start;
            _span = span;
            _label = label;
            _warnings = warnings;
        }

        public bool IsCancellationRequested => _progress.IsCancelled;

        public void OnLayerComposed(CompositionProgress progress, ComposedLayer layer)
        {
            float t = progress.Total > 0 ? (float)(progress.Index + 1) / progress.Total : 1f;
            _progress.Report(_start + _span * t, $"{_label}: {progress.Name} ({progress.Index + 1}/{progress.Total})");
        }

        public void OnWarning(string message) => _warnings.Add($"{_label}: {message}");
    }
}
