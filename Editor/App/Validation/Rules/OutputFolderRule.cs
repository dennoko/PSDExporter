using System;
using System.Collections.Generic;
using System.IO;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.Output;

namespace DennokoWorks.Tool.PSDExporter.App.Validation.Rules
{
    /// <summary>出力フォルダが指定され、存在するか。</summary>
    public sealed class OutputFolderRule : IProjectValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(ExportProject project)
        {
            var folder = project.Output.Folder;
            if (string.IsNullOrWhiteSpace(folder))
            {
                yield return ValidationMessage.Error("出力フォルダが指定されていません。");
                yield break;
            }

            string fullPath;
            try
            {
                fullPath = OutputPathResolver.ToFullPath(folder);
            }
            catch (Exception)
            {
                fullPath = null;
            }

            if (fullPath == null)
            {
                yield return ValidationMessage.Error($"出力フォルダのパスが不正です: {folder}");
            }
            else if (!Directory.Exists(fullPath))
            {
                yield return ValidationMessage.Error($"出力フォルダが存在しません: {folder}");
            }
        }
    }
}
