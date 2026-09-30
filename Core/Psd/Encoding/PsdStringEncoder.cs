using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding
{
    /// <summary>PSD の文字列表現 (Pascal 文字列 / Unicode 文字列) の書き込み。</summary>
    public static class PsdStringEncoder
    {
        private const int MaxPascalLength = 255;

        /// <summary>
        /// Pascal 文字列 (長さ 1 バイト + ASCII 本体) を書き、全体長が
        /// <paramref name="padTo"/> の倍数になるよう 0 でパディングする。
        /// 非 ASCII 文字は '?' に置き換える (正式名は 'luni' で別途保持する)。
        /// </summary>
        public static void WritePascal(BigEndianWriter writer, string value, int padTo)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            value = value ?? string.Empty;

            int length = Math.Min(value.Length, MaxPascalLength);
            writer.WriteByte((byte)length);
            for (int i = 0; i < length; i++)
            {
                char c = value[i];
                writer.WriteByte(c >= 0x20 && c < 0x7F ? (byte)c : (byte)'?');
            }

            int total = 1 + length;
            if (padTo > 1)
            {
                int remainder = total % padTo;
                if (remainder != 0) writer.WriteZeros(padTo - remainder);
            }
        }

        /// <summary>Unicode 文字列 (文字数 4 バイト + UTF-16BE)。</summary>
        public static void WriteUnicode(BigEndianWriter writer, string value)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            value = value ?? string.Empty;

            writer.WriteUInt32((uint)value.Length);
            foreach (char c in value) writer.WriteUInt16(c);
        }
    }
}
