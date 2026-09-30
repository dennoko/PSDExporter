using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.Output;

namespace DennokoWorks.Tool.PSDExporter.App.Validation.Rules
{
    /// <summary>複数スロットが同じファイル名に出力されないか。</summary>
    public sealed class DuplicateOutputNameRule : IProjectValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(ExportProject project)
        {
            if (project.Output.Overwrite == OverwritePolicy.AutoRename) yield break;

            var firstIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in project.EnumerateExportableSlots())
            {
                var fileName = OutputPathResolver.BuildFileName(project.Output.FilePrefix, entry.Slot.Suffix);
                if (firstIndexByName.TryGetValue(fileName, out var first))
                {
                    yield return ValidationMessage.Error(
                        $"スロット {first + 1} と {entry.Index + 1} の出力ファイル名が同じです ({fileName})。サフィックスを変更してください。",
                        slotIndex: entry.Index);
                }
                else
                {
                    firstIndexByName.Add(fileName, entry.Index);
                }
            }
        }
    }
}
