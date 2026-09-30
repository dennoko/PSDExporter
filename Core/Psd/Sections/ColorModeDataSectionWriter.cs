using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Sections
{
    /// <summary>Color Mode Data セクション。RGB モードでは空。</summary>
    internal static class ColorModeDataSectionWriter
    {
        public static void Write(BigEndianWriter writer)
        {
            writer.WriteUInt32(0);
        }
    }
}
