using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.TextureIO;

namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    /// <summary>マスクレイヤーから指定解像度のマスクを得る。</summary>
    public interface IMaskProvider
    {
        MaskImage GetMask(MaskLayerNode layer, int width, int height);
    }

    /// <summary>
    /// (マスクテクスチャ, チャンネル, 反転) ごとにネイティブ解像度のマスクを 1ch で保持し、
    /// 要求解像度へのリサイズ結果もキャッシュする。スロット間でマスクを共有するために使う。
    /// </summary>
    public sealed class MaskCache : IMaskProvider, IDisposable
    {
        private readonly ITextureSource _source;
        private readonly Dictionary<(int, MaskChannel, bool), MaskImage> _native = new Dictionary<(int, MaskChannel, bool), MaskImage>();
        private readonly Dictionary<(int, MaskChannel, bool, int, int), MaskImage> _resized = new Dictionary<(int, MaskChannel, bool, int, int), MaskImage>();

        public MaskCache(ITextureSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public MaskImage GetMask(MaskLayerNode layer, int width, int height)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            if (layer.Mask == null) throw new ExportException($"レイヤー「{layer.Name}」にマスクテクスチャが設定されていません。");

            int id = layer.Mask.GetInstanceID();
            var resizedKey = (id, layer.Channel, layer.InvertMask, width, height);
            if (_resized.TryGetValue(resizedKey, out var cached)) return cached;

            var nativeKey = (id, layer.Channel, layer.InvertMask);
            if (!_native.TryGetValue(nativeKey, out var native))
            {
                var rgba = _source.Read(layer.Mask);
                native = MaskExtractor.Extract(rgba, layer.Channel, layer.InvertMask);
                _native.Add(nativeKey, native);
            }

            var resized = BilinearResampler.Resize(native, width, height);
            _resized.Add(resizedKey, resized);
            return resized;
        }

        public void Dispose()
        {
            _native.Clear();
            _resized.Clear();
        }
    }
}
