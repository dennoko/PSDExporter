using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition.Renderers
{
    /// <summary>
    /// 透過ピクセル切り抜き: ソースのアルファにマスク値を乗算する。
    /// マスク 0 は完全透明、中間値は半透明として残す (アンチエイリアス境界の維持)。
    /// </summary>
    public sealed class TransparentCutoutRenderer : ILayerRenderer
    {
        public ComposedLayer Render(RgbaImage source, MaskImage mask)
        {
            var pixels = source.Clone();
            MaskOps.MultiplyAlpha(pixels, mask);
            return new ComposedLayer(pixels);
        }
    }
}
