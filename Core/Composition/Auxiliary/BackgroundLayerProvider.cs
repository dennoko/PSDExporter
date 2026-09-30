namespace DennokoWorks.Tool.PSDExporter.Core.Composition.Auxiliary
{
    /// <summary>最下層に切り抜きなしの元画像を置く。</summary>
    public sealed class BackgroundLayerProvider : IAuxiliaryLayerProvider
    {
        public AuxiliaryPlacement Placement => AuxiliaryPlacement.Bottom;
        public int Order => 0;

        public bool IsEnabled(CompositionOptions options) => options.IncludeBackground;

        public AuxiliaryLayer Create(AuxiliaryContext context)
        {
            var request = context.Request;
            return new AuxiliaryLayer(
                AuxiliaryLayerKeys.Background,
                request.Options.BackgroundLayerName,
                new ComposedLayer(request.Source));
        }
    }
}
