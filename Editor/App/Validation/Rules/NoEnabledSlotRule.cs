using System.Collections.Generic;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.Model;

namespace DennokoWorks.Tool.PSDExporter.App.Validation.Rules
{
    /// <summary>エクスポート可能なスロットが存在するか、有効スロットにテクスチャが設定されているか。</summary>
    public sealed class NoEnabledSlotRule : IProjectValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(ExportProject project)
        {
            for (int i = 0; i < project.Slots.Count; i++)
            {
                var slot = project.Slots[i];
                if (slot != null && slot.Enabled && slot.Texture == null)
                {
                    yield return ValidationMessage.Error($"スロット {i + 1} にテクスチャが設定されていません。", slotIndex: i);
                }
            }

            if (!project.EnumerateExportableSlots().Any())
            {
                yield return ValidationMessage.Error("エクスポートするテクスチャスロットがありません。");
            }
        }
    }
}
