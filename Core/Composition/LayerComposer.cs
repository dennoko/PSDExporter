using System;
using System.Collections.Generic;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.Core.Composition.Auxiliary;
using DennokoWorks.Tool.PSDExporter.Core.Composition.Renderers;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Core.Psd;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    /// <summary>
    /// <see cref="CompositionRequest"/> から <see cref="PsdDocument"/> を組み立てる。
    /// レイヤーを 1 枚ずつ生成 → 即チャンネル圧縮 → 生ピクセル破棄のストリーミング処理を行い、
    /// 同時にマージ済み画像を下から順に合成する。
    /// </summary>
    public sealed class LayerComposer
    {
        private readonly ILayerRenderer _renderer;
        private readonly IReadOnlyList<IAuxiliaryLayerProvider> _auxiliaryProviders;

        public LayerComposer(ILayerRenderer renderer, IEnumerable<IAuxiliaryLayerProvider> auxiliaryProviders)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _auxiliaryProviders = (auxiliaryProviders ?? Enumerable.Empty<IAuxiliaryLayerProvider>())
                .OrderBy(p => p.Placement)
                .ThenBy(p => p.Order)
                .ToList();
        }

        public static LayerComposer CreateDefault(CutoutMode mode)
        {
            return new LayerComposer(LayerRendererFactory.Create(mode), AuxiliaryLayerProviders.CreateDefault());
        }

        public PsdDocument Compose(CompositionRequest request, ICompositionObserver observer = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Source == null) throw new ArgumentException("Source image is required.", nameof(request));
            if (request.Root == null) throw new ArgumentException("Root group is required.", nameof(request));

            var source = request.Source;
            var options = request.Options;

            var maskLayers = LayerTreeFlattener.EnumerateMaskLayers(request.Root).ToList();
            ValidateMasks(maskLayers, source);

            var entries = LayerTreeFlattener.Flatten(request.Root);
            var auxiliary = _auxiliaryProviders.Where(p => p.IsEnabled(options)).ToList();

            var document = new PsdDocument(source.Width, source.Height);
            var merged = new RgbaImage(source.Width, source.Height);

            int total = auxiliary.Count + entries.Count;
            int index = 0;

            var context = new AuxiliaryContext(request, _renderer,
                maskLayers.Select(l => l.Mask).ToList(),
                message => observer?.OnWarning(message));

            foreach (var provider in auxiliary)
            {
                ThrowIfCancelled(observer);

                var aux = provider.Create(context);
                if (aux != null)
                {
                    AddPixelLayer(document, merged, aux.Name, aux.Layer, true, 1f, true, 1f, options);
                    observer?.OnLayerComposed(new CompositionProgress(index, total, aux.Key, aux.Name), aux.Layer);
                }
                index++;
            }

            foreach (var entry in entries)
            {
                ThrowIfCancelled(observer);

                var spec = entry.Spec;
                var progress = new CompositionProgress(index, total, spec.Key, spec.Name);

                switch (entry.Kind)
                {
                    case FlatEntryKind.GroupEnd:
                        document.Layers.Add(PsdLayerRecordFactory.CreateGroupEnd());
                        observer?.OnLayerComposed(progress, null);
                        break;

                    case FlatEntryKind.GroupBegin:
                        document.Layers.Add(PsdLayerRecordFactory.CreateGroupBegin((GroupSpec)spec));
                        observer?.OnLayerComposed(progress, null);
                        break;

                    case FlatEntryKind.Layer:
                        var maskLayer = (MaskLayerSpec)spec;
                        var composed = _renderer.Render(source, maskLayer.Mask);
                        AddPixelLayer(document, merged, spec.Name, composed,
                            spec.Visible, spec.Opacity, entry.EffectiveVisible, entry.EffectiveOpacity, options);
                        observer?.OnLayerComposed(progress, composed);
                        break;
                }

                index++;
            }

            document.MergedImage = merged;
            return document;
        }

        private static void AddPixelLayer(PsdDocument document, RgbaImage merged, string name, ComposedLayer layer,
            bool visible, float opacity, bool effectiveVisible, float effectiveOpacity, CompositionOptions options)
        {
            document.Layers.Add(PsdLayerRecordFactory.CreatePixelLayer(name, layer, visible, opacity, options.TrimLayerBounds));

            if (effectiveVisible)
            {
                AlphaCompositor.CompositeOver(merged, layer.Pixels, layer.UserMask, effectiveOpacity);
            }
        }

        private static void ValidateMasks(List<MaskLayerSpec> maskLayers, RgbaImage source)
        {
            foreach (var layer in maskLayers)
            {
                if (layer.Mask == null)
                    throw new ArgumentException($"Layer '{layer.Name}' has no mask.");
                if (!layer.Mask.SizeEquals(source.Width, source.Height))
                    throw new ArgumentException(
                        $"Mask of layer '{layer.Name}' ({layer.Mask.Width}x{layer.Mask.Height}) " +
                        $"does not match the source ({source.Width}x{source.Height}).");
            }
        }

        private static void ThrowIfCancelled(ICompositionObserver observer)
        {
            if (observer != null && observer.IsCancellationRequested) throw new OperationCanceledException();
        }
    }
}
