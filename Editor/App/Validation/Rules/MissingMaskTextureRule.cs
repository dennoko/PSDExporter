using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Model;

namespace DennokoWorks.Tool.PSDExporter.App.Validation.Rules
{
    /// <summary>エクスポート対象のレイヤーにマスクが設定されているか。</summary>
    public sealed class MissingMaskTextureRule : IProjectValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(ExportProject project)
        {
            foreach (var layer in LayerTreeOperations.EnumerateMaskLayers(project.Root, exportedOnly: true))
            {
                if (layer.Mask == null)
                {
                    yield return ValidationMessage.Error($"レイヤー「{DisplayName(layer)}」にマスクテクスチャが設定されていません。", layer.Id);
                }
            }
        }

        private static string DisplayName(LayerTreeNode node) => string.IsNullOrWhiteSpace(node.Name) ? "(名前なし)" : node.Name;
    }
}
