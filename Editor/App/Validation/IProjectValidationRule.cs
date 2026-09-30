using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Model;

namespace DennokoWorks.Tool.PSDExporter.App.Validation
{
    /// <summary>検証ルール 1 つ。ルールの追加はこのインターフェースの実装を追加するだけで済む。</summary>
    public interface IProjectValidationRule
    {
        IEnumerable<ValidationMessage> Validate(ExportProject project);
    }
}
