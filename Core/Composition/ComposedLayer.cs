using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    /// <summary>生成済みの 1 レイヤー分のピクセル。PSD 化された後は破棄される。</summary>
    public sealed class ComposedLayer
    {
        public RgbaImage Pixels { get; }

        /// <summary>PSD のユーザーレイヤーマスクとして付与するマスク。null ならなし。</summary>
        public MaskImage UserMask { get; }

        public ComposedLayer(RgbaImage pixels, MaskImage userMask = null)
        {
            Pixels = pixels;
            UserMask = userMask;
        }
    }
}
