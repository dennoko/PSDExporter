using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Sections
{
    /// <summary>
    /// Image Data セクション (マージ済み画像)。RLE 圧縮で、全チャンネル全行のバイト数テーブルの後に
    /// R, G, B, A の順でデータを書く。
    /// </summary>
    internal static class ImageDataSectionWriter
    {
        public static void Write(BigEndianWriter writer, PsdDocument document)
        {
            writer.WriteUInt16(PsdChannelData.CompressionRle);

            var channels = new RleChannel[HeaderSectionWriter.ChannelCount];
            var merged = document.MergedImage;
            var rect = PsdRect.FromSize(document.Width, document.Height);

            for (int c = 0; c < channels.Length; c++)
            {
                channels[c] = merged != null
                    ? ChannelEncoder.EncodeRle(merged.Pixels, merged.Width, merged.Height, RgbaImage.Channels, c, rect)
                    : EncodeZeroChannel(document.Width, document.Height);
            }

            foreach (var channel in channels)
            {
                foreach (var rowLength in channel.RowLengths) writer.WriteUInt16(rowLength);
            }

            foreach (var channel in channels) writer.WriteBytes(channel.Data);
        }

        /// <summary>全画素 0 のチャンネル。巨大な空バッファを確保せず 1 行分の符号を繰り返す。</summary>
        private static RleChannel EncodeZeroChannel(int width, int height)
        {
            var row = new byte[width];
            var packed = new byte[PackBitsEncoder.MaxEncodedLength(width)];
            int len = PackBitsEncoder.Encode(row, 0, width, packed, 0);

            var rowLengths = new ushort[height];
            var data = new byte[len * height];
            for (int y = 0; y < height; y++)
            {
                rowLengths[y] = (ushort)len;
                System.Buffer.BlockCopy(packed, 0, data, y * len, len);
            }

            return new RleChannel(rowLengths, data);
        }
    }
}
