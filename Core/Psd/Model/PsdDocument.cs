using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    /// <summary>
    /// 書き出し直前の PSD の内容。レイヤーは PSD 仕様どおり「下 → 上」の順に並べ、
    /// チャンネルは圧縮済みで保持する。
    /// </summary>
    public sealed class PsdDocument
    {
        public const int MaxDimension = 30000;

        public int Width { get; }
        public int Height { get; }

        public List<PsdLayerRecord> Layers { get; } = new List<PsdLayerRecord>();

        /// <summary>マージ済みプレビュー画像 (Image Data セクション)。null なら透明で書き出す。</summary>
        public RgbaImage MergedImage { get; set; }

        public PsdDocument(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
