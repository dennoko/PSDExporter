namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    /// <summary>マスクをレイヤーにどう適用するか。</summary>
    public enum CutoutMode
    {
        /// <summary>マスク外を透明にした通常レイヤーとして出力する。</summary>
        TransparentCutout = 0,

        /// <summary>ピクセルはそのまま、マスクを PSD のレイヤーマスクとして付与する。</summary>
        LayerMask = 1,
    }
}
