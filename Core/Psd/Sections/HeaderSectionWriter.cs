using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Sections
{
    /// <summary>File Header セクション (26 バイト)。</summary>
    internal static class HeaderSectionWriter
    {
        public const ushort ChannelCount = 4; // RGBA
        private const ushort Version = 1;
        private const ushort Depth = 8;
        private const ushort ColorModeRgb = 3;

        public static void Write(BigEndianWriter writer, PsdDocument document)
        {
            writer.WriteSignature("8BPS");
            writer.WriteUInt16(Version);
            writer.WriteZeros(6);
            writer.WriteUInt16(ChannelCount);
            writer.WriteUInt32((uint)document.Height);
            writer.WriteUInt32((uint)document.Width);
            writer.WriteUInt16(Depth);
            writer.WriteUInt16(ColorModeRgb);
        }
    }
}
