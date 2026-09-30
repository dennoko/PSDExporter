using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Model;

namespace DennokoWorks.Tool.PSDExporter.App.Validation.Rules
{
    /// <summary>名前が空のノード (既定名で補完される)。</summary>
    public sealed class EmptyLayerNameRule : IProjectValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(ExportProject project)
        {
            foreach (var node in LayerTreeOperations.EnumerateDescendants(project.Root))
            {
                if (!node.IncludeInExport || !string.IsNullOrWhiteSpace(node.Name)) continue;

                var kind = node is GroupNode ? "グループ" : "レイヤー";
                yield return ValidationMessage.Warning($"名前が空の{kind}があります。既定名で出力されます。", node.Id);
            }
        }
    }
}
