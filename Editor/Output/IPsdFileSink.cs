using DennokoWorks.Tool.PSDExporter.Core.Psd;

namespace DennokoWorks.Tool.PSDExporter.Output
{
    /// <summary>完成した PSD ドキュメントの保存先。</summary>
    public interface IPsdFileSink
    {
        void Write(ResolvedOutput output, PsdDocument document);
    }
}
