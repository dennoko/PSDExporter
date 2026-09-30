using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    /// <summary>合成用の (Unity 非依存な) レイヤーツリーのノード。</summary>
    public abstract class LayerSpec
    {
        /// <summary>呼び出し側がノードを識別するためのキー (プレビューの対応付け等)。</summary>
        public string Key { get; set; }

        public string Name { get; set; }
        public bool Visible { get; set; } = true;

        /// <summary>0..1</summary>
        public float Opacity { get; set; } = 1f;
    }

    public sealed class GroupSpec : LayerSpec
    {
        public bool Expanded { get; set; } = true;

        /// <summary>子ノード。UI と同じく「上 → 下」の順。</summary>
        public List<LayerSpec> Children { get; } = new List<LayerSpec>();
    }

    public sealed class MaskLayerSpec : LayerSpec
    {
        /// <summary>ソース画像と同じ解像度にリサイズ済みのマスク。</summary>
        public MaskImage Mask { get; set; }
    }
}
