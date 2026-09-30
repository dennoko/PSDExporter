using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition.Renderers
{
    /// <summary>マスクレイヤー 1 枚分のピクセルを生成する戦略 (切り抜き方式)。</summary>
    public interface ILayerRenderer
    {
        /// <param name="source">ソース画像。変更してはならない。</param>
        /// <param name="mask">ソースと同じ解像度のマスク。</param>
        ComposedLayer Render(RgbaImage source, MaskImage mask);
    }
}
