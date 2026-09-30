using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Composition.Renderers;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition.Auxiliary
{
    public enum AuxiliaryPlacement
    {
        /// <summary>ユーザー定義レイヤーより下 (ドキュメント最下層側)。</summary>
        Bottom = 0,
    }

    /// <summary>自動生成レイヤーに渡される情報。</summary>
    public sealed class AuxiliaryContext
    {
        public CompositionRequest Request { get; }
        public ILayerRenderer Renderer { get; }

        /// <summary>エクスポート対象の全マスクレイヤーのマスク (非表示レイヤーも含む)。</summary>
        public IReadOnlyList<MaskImage> AllMasks { get; }

        private readonly Action<string> _reportWarning;

        public AuxiliaryContext(CompositionRequest request, ILayerRenderer renderer,
            IReadOnlyList<MaskImage> allMasks, Action<string> reportWarning)
        {
            Request = request;
            Renderer = renderer;
            AllMasks = allMasks;
            _reportWarning = reportWarning;
        }

        public void ReportWarning(string message) => _reportWarning?.Invoke(message);
    }

    public sealed class AuxiliaryLayer
    {
        public string Key { get; }
        public string Name { get; }
        public ComposedLayer Layer { get; }

        public AuxiliaryLayer(string key, string name, ComposedLayer layer)
        {
            Key = key;
            Name = name;
            Layer = layer;
        }
    }

    /// <summary>ユーザー定義ではない自動生成レイヤー (背景・未分類など) の拡張点。</summary>
    public interface IAuxiliaryLayerProvider
    {
        AuxiliaryPlacement Placement { get; }

        /// <summary>同じ Placement 内での並び順。小さいほど下。</summary>
        int Order { get; }

        bool IsEnabled(CompositionOptions options);

        /// <summary>レイヤーを生成する。出力しない場合は null。</summary>
        AuxiliaryLayer Create(AuxiliaryContext context);
    }

    public static class AuxiliaryLayerKeys
    {
        public const string Background = "__background__";
        public const string Unassigned = "__unassigned__";
    }

    public static class AuxiliaryLayerProviders
    {
        public static IReadOnlyList<IAuxiliaryLayerProvider> CreateDefault()
        {
            return new IAuxiliaryLayerProvider[]
            {
                new BackgroundLayerProvider(),
                new UnassignedLayerProvider(),
            };
        }
    }
}
