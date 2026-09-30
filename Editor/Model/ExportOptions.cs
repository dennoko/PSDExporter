using System;
using DennokoWorks.Tool.PSDExporter.Core.Composition;

namespace DennokoWorks.Tool.PSDExporter.Model
{
    /// <summary>テクスチャのピクセルをどの品質で読み込むか。</summary>
    public enum SourceReadQuality
    {
        /// <summary>インポート設定 (圧縮・最大サイズ等) を一時的に解除し、元ファイルの品質で読む。</summary>
        Original = 0,

        /// <summary>現在のインポート結果をそのまま読む (Read/Write のみ一時有効化)。</summary>
        AsImported = 1,
    }

    [Serializable]
    public sealed class ExportOptions
    {
        public CutoutMode CutoutMode = CutoutMode.TransparentCutout;
        public bool IncludeBackground = true;
        public bool IncludeUnassigned = true;
        public string BackgroundLayerName = CompositionOptions.DefaultBackgroundLayerName;
        public string UnassignedLayerName = CompositionOptions.DefaultUnassignedLayerName;
        public bool TrimLayerBounds;
        public SourceReadQuality ReadQuality = SourceReadQuality.Original;
    }
}
