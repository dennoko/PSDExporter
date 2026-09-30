using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Composition;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Model;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    /// <summary>
    /// Editor のモデル (Texture2D 参照) を Core の合成リクエスト (ピクセル) に変換する。
    /// </summary>
    public static class ProjectResolver
    {
        public const string DefaultLayerName = "Layer";
        public const string DefaultGroupName = "Group";

        public static CompositionRequest Resolve(ExportProject project, RgbaImage source, IMaskProvider masks)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (masks == null) throw new ArgumentNullException(nameof(masks));

            var root = new GroupSpec { Key = project.Root.Id, Name = project.Root.Name };
            AppendChildren(project.Root, root, source, masks);
            return new CompositionRequest(source, root, ToCompositionOptions(project.Options));
        }

        public static CompositionOptions ToCompositionOptions(ExportOptions options)
        {
            return new CompositionOptions
            {
                CutoutMode = options.CutoutMode,
                IncludeBackground = options.IncludeBackground,
                IncludeUnassigned = options.IncludeUnassigned,
                BackgroundLayerName = NameOr(options.BackgroundLayerName, CompositionOptions.DefaultBackgroundLayerName),
                UnassignedLayerName = NameOr(options.UnassignedLayerName, CompositionOptions.DefaultUnassignedLayerName),
                TrimLayerBounds = options.TrimLayerBounds,
            };
        }

        /// <summary>エクスポートに必要な全テクスチャ (スロット + エクスポート対象のマスク)。</summary>
        public static List<Texture2D> CollectTextures(ExportProject project, IEnumerable<SlotEntry> slots)
        {
            var textures = new List<Texture2D>();
            foreach (var entry in slots) textures.Add(entry.Slot.Texture);

            foreach (var node in EnumerateExportedMaskLayers(project.Root))
            {
                if (node.Mask != null) textures.Add(node.Mask);
            }
            return textures;
        }

        private static void AppendChildren(GroupNode node, GroupSpec spec, RgbaImage source, IMaskProvider masks)
        {
            foreach (var child in node.Children)
            {
                if (child == null || !child.IncludeInExport) continue;

                switch (child)
                {
                    case GroupNode group:
                        var groupSpec = new GroupSpec
                        {
                            Key = group.Id,
                            Name = NameOr(group.Name, DefaultGroupName),
                            Visible = group.Visible,
                            Opacity = group.Opacity,
                            Expanded = group.Expanded,
                        };
                        AppendChildren(group, groupSpec, source, masks);
                        spec.Children.Add(groupSpec);
                        break;

                    case MaskLayerNode layer:
                        spec.Children.Add(new MaskLayerSpec
                        {
                            Key = layer.Id,
                            Name = NameOr(layer.Name, DefaultLayerName),
                            Visible = layer.Visible,
                            Opacity = layer.Opacity,
                            Mask = masks.GetMask(layer, source.Width, source.Height),
                        });
                        break;
                }
            }
        }

        private static IEnumerable<MaskLayerNode> EnumerateExportedMaskLayers(GroupNode node)
        {
            foreach (var child in node.Children)
            {
                if (child == null || !child.IncludeInExport) continue;

                if (child is MaskLayerNode layer) yield return layer;
                else if (child is GroupNode group)
                {
                    foreach (var nested in EnumerateExportedMaskLayers(group)) yield return nested;
                }
            }
        }

        private static string NameOr(string name, string fallback) => string.IsNullOrWhiteSpace(name) ? fallback : name;
    }
}
