using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Sections
{
    /// <summary>Image Resources セクションに書き込むリソース 1 件。</summary>
    public interface IImageResource
    {
        ushort Id { get; }
        byte[] GetData();
    }

    /// <summary>Image Resources セクション。初期版ではリソースを書き込まない (長さ 0)。</summary>
    internal static class ImageResourcesSectionWriter
    {
        public static void Write(BigEndianWriter writer, IReadOnlyList<IImageResource> resources)
        {
            using (writer.BeginLength())
            {
                if (resources == null) return;

                foreach (var resource in resources)
                {
                    writer.WriteSignature("8BIM");
                    writer.WriteUInt16(resource.Id);
                    PsdStringEncoder.WritePascal(writer, string.Empty, 2);
                    using (writer.BeginLength(2))
                    {
                        writer.WriteBytes(resource.GetData());
                    }
                }
            }
        }
    }
}
