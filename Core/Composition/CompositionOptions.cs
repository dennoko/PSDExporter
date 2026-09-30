namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    public sealed class CompositionOptions
    {
        public const string DefaultBackgroundLayerName = "Background / 元画像";
        public const string DefaultUnassignedLayerName = "Base / 未分類";

        public CutoutMode CutoutMode { get; set; } = CutoutMode.TransparentCutout;
        public bool IncludeBackground { get; set; } = true;
        public bool IncludeUnassigned { get; set; } = true;
        public string BackgroundLayerName { get; set; } = DefaultBackgroundLayerName;
        public string UnassignedLayerName { get; set; } = DefaultUnassignedLayerName;

        /// <summary>
        /// 透明領域をトリムしてレイヤー矩形を縮める (ファイルサイズ削減)。
        /// レイヤーマスク付きレイヤーは編集性を保つためトリムしない。
        /// </summary>
        public bool TrimLayerBounds { get; set; }
    }
}
