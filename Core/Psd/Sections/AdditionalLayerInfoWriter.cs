using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Sections
{
    /// <summary>レイヤーレコード末尾の Additional Layer Information ブロック。</summary>
    internal static class AdditionalLayerInfoWriter
    {
        /// <summary>'luni': Unicode のレイヤー名 (日本語名を正しく保持するため)。</summary>
        public static void WriteUnicodeName(BigEndianWriter writer, string name)
        {
            writer.WriteSignature("8BIM");
            writer.WriteSignature("luni");
            using (writer.BeginLength(2))
            {
                PsdStringEncoder.WriteUnicode(writer, name);
            }
        }

        /// <summary>'lsct': グループ (フォルダ) の開始/終端情報。</summary>
        public static void WriteSectionDivider(BigEndianWriter writer, PsdSectionType type, PsdBlendMode blendMode)
        {
            writer.WriteSignature("8BIM");
            writer.WriteSignature("lsct");
            using (writer.BeginLength())
            {
                writer.WriteUInt32((uint)type);
                if (type != PsdSectionType.BoundingDivider)
                {
                    writer.WriteSignature("8BIM");
                    writer.WriteSignature(blendMode.ToKey());
                }
            }
        }
    }
}
