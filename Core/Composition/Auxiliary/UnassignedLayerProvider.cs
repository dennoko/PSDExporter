using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition.Auxiliary
{
    /// <summary>
    /// どのマスクにも含まれない領域 (= 全マスク合併の反転) を「未分類」レイヤーとして生成する。
    /// 切り抜き方式は通常レイヤーと同じ Renderer を使い、出力の一貫性を保つ。
    /// </summary>
    public sealed class UnassignedLayerProvider : IAuxiliaryLayerProvider
    {
        public AuxiliaryPlacement Placement => AuxiliaryPlacement.Bottom;
        public int Order => 1;

        public bool IsEnabled(CompositionOptions options) => options.IncludeUnassigned;

        public AuxiliaryLayer Create(AuxiliaryContext context)
        {
            var request = context.Request;
            var source = request.Source;

            var union = MaskOps.Union(context.AllMasks, source.Width, source.Height);
            var unassigned = MaskOps.Invert(union);

            if (unassigned.IsAllZero())
            {
                context.ReportWarning("未分類領域がありません (全画素がいずれかのマスクに含まれています)。未分類レイヤーは空になります。");
            }

            return new AuxiliaryLayer(
                AuxiliaryLayerKeys.Unassigned,
                request.Options.UnassignedLayerName,
                context.Renderer.Render(source, unassigned));
        }
    }
}
