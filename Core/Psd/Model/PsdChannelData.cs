using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    /// <summary>レイヤーの 1 チャンネル分の (圧縮済み) 画像データ。</summary>
    public sealed class PsdChannelData
    {
        public const short Red = 0;
        public const short Green = 1;
        public const short Blue = 2;
        public const short Transparency = -1;
        public const short UserMask = -2;

        public const ushort CompressionRaw = 0;
        public const ushort CompressionRle = 1;

        public short Id { get; }
        public ushort Compression { get; }

        /// <summary>
        /// 圧縮方式 2 バイトを除いたデータ本体。RLE の場合は行バイト数テーブル + 圧縮データ。
        /// </summary>
        public byte[] EncodedBytes { get; }

        /// <summary>レイヤーレコードに書く「チャンネルデータ長」(圧縮方式 2 バイトを含む)。</summary>
        public int Length => 2 + EncodedBytes.Length;

        public PsdChannelData(short id, ushort compression, byte[] encodedBytes)
        {
            Id = id;
            Compression = compression;
            EncodedBytes = encodedBytes ?? throw new ArgumentNullException(nameof(encodedBytes));
        }

        /// <summary>画像を持たない (空矩形の) チャンネル。</summary>
        public static PsdChannelData Empty(short id) => new PsdChannelData(id, CompressionRaw, Array.Empty<byte>());
    }
}
