using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    /// <summary>1 枚のソース画像から 1 つの PSD を合成するための入力一式。</summary>
    public sealed class CompositionRequest
    {
        public RgbaImage Source { get; }

        /// <summary>仮想ルート。ルート自身はレイヤーとして出力されない。</summary>
        public GroupSpec Root { get; }

        public CompositionOptions Options { get; }

        public CompositionRequest(RgbaImage source, GroupSpec root, CompositionOptions options)
        {
            Source = source;
            Root = root;
            Options = options ?? new CompositionOptions();
        }
    }
}
