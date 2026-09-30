namespace DennokoWorks.Tool.PSDExporter.Core.Imaging
{
    /// <summary>マスク画像のどの成分をマスク値として使うか。</summary>
    public enum MaskChannel
    {
        /// <summary>輝度 (白黒マスク)。透明部分は非選択として扱う (輝度 × アルファ)。</summary>
        Luminance = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        /// <summary>アルファ (アルファマスク)。</summary>
        Alpha = 4,
    }
}
