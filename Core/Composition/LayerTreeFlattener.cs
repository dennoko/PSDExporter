using System;
using System.Collections.Generic;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    public enum FlatEntryKind
    {
        Layer,
        GroupBegin,
        GroupEnd,
    }

    /// <summary>平坦化されたツリーの 1 要素 (PSD レコード 1 件に対応)。</summary>
    public readonly struct FlatEntry
    {
        public readonly FlatEntryKind Kind;
        public readonly LayerSpec Spec;

        /// <summary>親グループの表示状態を加味した表示状態 (マージ画像の合成に使う)。</summary>
        public readonly bool EffectiveVisible;

        /// <summary>親グループの不透明度を掛け合わせた不透明度 (マージ画像の合成に使う)。</summary>
        public readonly float EffectiveOpacity;

        public FlatEntry(FlatEntryKind kind, LayerSpec spec, bool effectiveVisible, float effectiveOpacity)
        {
            Kind = kind;
            Spec = spec;
            EffectiveVisible = effectiveVisible;
            EffectiveOpacity = effectiveOpacity;
        }
    }

    /// <summary>
    /// レイヤーツリーを PSD のレコード順 (下 → 上) に平坦化する。グループは
    /// [GroupEnd (区切り), 子 (下 → 上)..., GroupBegin (グループ本体)] として表現する。
    /// </summary>
    public static class LayerTreeFlattener
    {
        public static List<FlatEntry> Flatten(GroupSpec root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            var result = new List<FlatEntry>();
            AppendChildren(root, true, 1f, result);
            return result;
        }

        /// <summary>ツリー内の全マスクレイヤーを列挙する (表示状態に関係なく)。</summary>
        public static IEnumerable<MaskLayerSpec> EnumerateMaskLayers(GroupSpec root)
        {
            if (root == null) yield break;

            foreach (var child in root.Children)
            {
                if (child is MaskLayerSpec layer)
                {
                    yield return layer;
                }
                else if (child is GroupSpec group)
                {
                    foreach (var nested in EnumerateMaskLayers(group)) yield return nested;
                }
            }
        }

        private static void AppendChildren(GroupSpec group, bool parentVisible, float parentOpacity, List<FlatEntry> result)
        {
            // Children は「上 → 下」なので逆順にたどって「下 → 上」にする
            for (int i = group.Children.Count - 1; i >= 0; i--)
            {
                var child = group.Children[i];
                if (child == null) continue;

                bool visible = parentVisible && child.Visible;
                float opacity = parentOpacity * Clamp01(child.Opacity);

                switch (child)
                {
                    case GroupSpec childGroup:
                        result.Add(new FlatEntry(FlatEntryKind.GroupEnd, childGroup, visible, opacity));
                        AppendChildren(childGroup, visible, opacity, result);
                        result.Add(new FlatEntry(FlatEntryKind.GroupBegin, childGroup, visible, opacity));
                        break;
                    case MaskLayerSpec layer:
                        result.Add(new FlatEntry(FlatEntryKind.Layer, layer, visible, opacity));
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported layer spec: {child.GetType().Name}");
                }
            }
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
