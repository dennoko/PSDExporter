using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition.Renderers
{
    /// <summary>
    /// レイヤーマスク保持: ピクセルはソースのまま (参照を共有、複製しない) にし、
    /// マスクを PSD のユーザーレイヤーマスクとして付与する。
    /// </summary>
    public sealed class LayerMaskRenderer : ILayerRenderer
    {
        public ComposedLayer Render(RgbaImage source, MaskImage mask)
        {
            return new ComposedLayer(source, mask);
        }
    }
}
