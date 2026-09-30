using System;
using System.IO;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding
{
    /// <summary>行ごとに PackBits 圧縮した 1 チャンネル分のデータ。</summary>
    public sealed class RleChannel
    {
        public ushort[] RowLengths { get; }
        public byte[] Data { get; }

        public RleChannel(ushort[] rowLengths, byte[] data)
        {
            RowLengths = rowLengths;
            Data = data;
        }
    }

    /// <summary>画像バッファから PSD のチャンネルデータを生成する。</summary>
    public static class ChannelEncoder
    {
        /// <summary>レイヤー用チャンネル (RGBA のいずれか) を指定矩形で圧縮する。</summary>
        public static PsdChannelData EncodeLayerChannel(RgbaImage image, short channelId, PsdRect rect)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (rect.IsEmpty) return PsdChannelData.Empty(channelId);

            var rle = EncodeRle(image.Pixels, image.Width, image.Height, RgbaImage.Channels, ComponentOf(channelId), rect);
            return new PsdChannelData(channelId, PsdChannelData.CompressionRle, ToLayerBytes(rle));
        }

        /// <summary>ユーザーマスク (-2) チャンネルを指定矩形で圧縮する。</summary>
        public static PsdChannelData EncodeMaskChannel(MaskImage mask, PsdRect rect)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            if (rect.IsEmpty) return PsdChannelData.Empty(PsdChannelData.UserMask);

            var rle = EncodeRle(mask.Values, mask.Width, mask.Height, 1, 0, rect);
            return new PsdChannelData(PsdChannelData.UserMask, PsdChannelData.CompressionRle, ToLayerBytes(rle));
        }

        /// <summary>
        /// インターリーブされたバッファの 1 成分を行単位で PackBits 圧縮する。
        /// </summary>
        public static RleChannel EncodeRle(byte[] src, int width, int height, int stride, int component, PsdRect rect)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (rect.Left < 0 || rect.Top < 0 || rect.Right > width || rect.Bottom > height)
                throw new ArgumentOutOfRangeException(nameof(rect), "Rect exceeds the image bounds.");

            int rw = rect.Width;
            int rh = rect.Height;
            var rowLengths = new ushort[rh];
            var row = new byte[rw];
            var packed = new byte[PackBitsEncoder.MaxEncodedLength(rw)];

            using (var ms = new MemoryStream(Math.Max(16, rw * rh / 4)))
            {
                for (int y = 0; y < rh; y++)
                {
                    int p = ((rect.Top + y) * width + rect.Left) * stride + component;
                    for (int x = 0; x < rw; x++, p += stride) row[x] = src[p];

                    int len = PackBitsEncoder.Encode(row, 0, rw, packed, 0);
                    if (len > ushort.MaxValue) throw new InvalidOperationException("Row is too long for PSD RLE.");

                    rowLengths[y] = (ushort)len;
                    ms.Write(packed, 0, len);
                }

                return new RleChannel(rowLengths, ms.ToArray());
            }
        }

        /// <summary>レイヤーチャンネル形式 ([行バイト数 × 行数][データ]) に連結する。</summary>
        public static byte[] ToLayerBytes(RleChannel rle)
        {
            int headerLength = rle.RowLengths.Length * 2;
            var bytes = new byte[headerLength + rle.Data.Length];

            for (int i = 0; i < rle.RowLengths.Length; i++)
            {
                bytes[i * 2] = (byte)(rle.RowLengths[i] >> 8);
                bytes[i * 2 + 1] = (byte)rle.RowLengths[i];
            }

            Buffer.BlockCopy(rle.Data, 0, bytes, headerLength, rle.Data.Length);
            return bytes;
        }

        private static int ComponentOf(short channelId)
        {
            switch (channelId)
            {
                case PsdChannelData.Red: return 0;
                case PsdChannelData.Green: return 1;
                case PsdChannelData.Blue: return 2;
                case PsdChannelData.Transparency: return 3;
                default: throw new ArgumentOutOfRangeException(nameof(channelId), channelId, "Not an RGBA channel.");
            }
        }
    }
}
