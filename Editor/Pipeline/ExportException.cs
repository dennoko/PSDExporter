using System;

namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    /// <summary>ユーザーに提示できる理由でエクスポートが続行できない場合の例外。</summary>
    public sealed class ExportException : Exception
    {
        public ExportException(string message) : base(message)
        {
        }

        public ExportException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
