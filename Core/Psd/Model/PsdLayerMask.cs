namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    /// <summary>
    /// ユーザーレイヤーマスクのメタ情報。画素データはチャンネル ID -2 として
    /// <see cref="PsdLayerRecord.Channels"/> に格納する。
    /// </summary>
    public sealed class PsdLayerMask
    {
        public PsdRect Bounds { get; set; }

        /// <summary>矩形外のマスク値 (0 = 隠す, 255 = 表示)。</summary>
        public byte DefaultColor { get; set; }

        public bool Disabled { get; set; }

        public PsdLayerMask(PsdRect bounds, byte defaultColor = 0)
        {
            Bounds = bounds;
            DefaultColor = defaultColor;
        }
    }
}
