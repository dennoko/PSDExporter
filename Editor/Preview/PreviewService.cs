using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Composition;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.Pipeline;
using DennokoWorks.Tool.PSDExporter.TextureIO;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Preview
{
    /// <summary>
    /// 縮小解像度でエクスポートと同じ合成処理を実行し、レイヤーのサムネイルと全体プレビューを生成する。
    /// 再インポートを伴うため自動更新はせず、<see cref="Refresh"/> 呼び出し時のみ生成する。
    /// </summary>
    public sealed class PreviewService : IDisposable
    {
        public const string CompositeKey = "__composite__";

        private readonly ExportPipeline.TextureSourceFactory _sourceFactory;
        private readonly PreviewTextureCache _cache = new PreviewTextureCache();
        private readonly List<string> _warnings = new List<string>();

        public PreviewService(ExportPipeline.TextureSourceFactory sourceFactory)
        {
            _sourceFactory = sourceFactory ?? throw new ArgumentNullException(nameof(sourceFactory));
        }

        public static PreviewService CreateDefault()
        {
            return new PreviewService((textures, quality) => TextureAccessSession.Open(textures, quality));
        }

        /// <summary>全体プレビューの長辺の最大ピクセル数。</summary>
        public int PreviewMaxSize { get; set; } = 512;

        /// <summary>レイヤーサムネイルの長辺の最大ピクセル数。</summary>
        public int ThumbnailMaxSize { get; set; } = 64;

        /// <summary>プレビュー生成時の読み込み品質。再インポートを減らすため既定は AsImported。</summary>
        public SourceReadQuality ReadQuality { get; set; } = SourceReadQuality.AsImported;

        /// <summary>最後に生成したときのモデルのリビジョン (未生成なら -1)。</summary>
        public int GeneratedRevision { get; private set; } = -1;

        public int GeneratedSlotIndex { get; private set; } = -1;

        public string LastError { get; private set; }

        public IReadOnlyList<string> Warnings => _warnings;

        public Texture2D Composite => _cache.Get(CompositeKey);

        /// <summary>レイヤーノード Id (または補助レイヤーのキー) に対応するサムネイル。</summary>
        public Texture2D GetThumbnail(string key) => _cache.Get(key);

        public bool IsStale(int revision, int slotIndex) => GeneratedRevision != revision || GeneratedSlotIndex != slotIndex;

        /// <returns>生成に成功したら true。失敗時は <see cref="LastError"/> に理由を格納する。</returns>
        public bool Refresh(ExportProject project, int slotIndex, int revision)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            _cache.Clear();
            _warnings.Clear();
            LastError = null;
            GeneratedRevision = revision;
            GeneratedSlotIndex = slotIndex;

            if (slotIndex < 0 || slotIndex >= project.Slots.Count || project.Slots[slotIndex]?.Texture == null)
            {
                LastError = "プレビューするテクスチャスロットがありません。";
                return false;
            }

            var slot = project.Slots[slotIndex];

            try
            {
                var textures = ProjectResolver.CollectTextures(project, new[] { new SlotEntry(slotIndex, slot) });
                using (var source = _sourceFactory(textures, ReadQuality))
                using (var masks = new MaskCache(source))
                {
                    var small = ReadDownscaled(source, slot.Texture);
                    var request = ProjectResolver.Resolve(project, small, masks);

                    var observer = new ThumbnailObserver(this);
                    var document = LayerComposer.CreateDefault(project.Options.CutoutMode).Compose(request, observer);

                    _cache.Set(CompositeKey, document.MergedImage);
                    _warnings.AddRange(source.Warnings);
                }
                return true;
            }
            catch (Exception e)
            {
                _cache.Clear();
                LastError = e is ExportException ? e.Message : $"{e.GetType().Name}: {e.Message}";
                return false;
            }
        }

        public void Clear()
        {
            _cache.Clear();
            _warnings.Clear();
            LastError = null;
            GeneratedRevision = -1;
            GeneratedSlotIndex = -1;
        }

        public void Dispose() => _cache.Dispose();

        private RgbaImage ReadDownscaled(ITextureSource source, Texture2D texture)
        {
            var full = source.Read(texture);
            BilinearResampler.FitWithin(full.Width, full.Height, PreviewMaxSize, out var w, out var h);
            return BilinearResampler.Resize(full, w, h);
        }

        private void StoreThumbnail(string key, ComposedLayer layer)
        {
            if (key == null || layer == null) return;

            var pixels = layer.Pixels;
            var flattened = new RgbaImage(pixels.Width, pixels.Height);
            AlphaCompositor.CompositeOver(flattened, pixels, layer.UserMask, 1f);

            BilinearResampler.FitWithin(flattened.Width, flattened.Height, ThumbnailMaxSize, out var w, out var h);
            _cache.Set(key, BilinearResampler.Resize(flattened, w, h));
        }

        private sealed class ThumbnailObserver : ICompositionObserver
        {
            private readonly PreviewService _owner;

            public ThumbnailObserver(PreviewService owner)
            {
                _owner = owner;
            }

            public bool IsCancellationRequested => false;

            public void OnLayerComposed(CompositionProgress progress, ComposedLayer layer)
            {
                _owner.StoreThumbnail(progress.Key, layer);
            }

            public void OnWarning(string message) => _owner._warnings.Add(message);
        }
    }
}
