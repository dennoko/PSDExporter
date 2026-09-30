using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding
{
    /// <summary>
    /// PackBits (Apple RLE) エンコーダ。3 バイト以上の連続のみランとして扱うことで、
    /// 出力長が常に <see cref="MaxEncodedLength"/> 以下になることを保証する。
    /// </summary>
    public static class PackBitsEncoder
    {
        private const int MaxPacket = 128;

        /// <summary>count バイトを符号化したときの最大長。</summary>
        public static int MaxEncodedLength(int count) => count + (count + MaxPacket - 1) / MaxPacket;

        /// <summary>src[offset..offset+count) を dst[dstOffset..] に符号化し、書き込んだバイト数を返す。</summary>
        public static int Encode(byte[] src, int offset, int count, byte[] dst, int dstOffset)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dst == null) throw new ArgumentNullException(nameof(dst));
            if (dst.Length - dstOffset < MaxEncodedLength(count))
                throw new ArgumentException("Destination buffer is too small.", nameof(dst));

            int i = 0;
            int d = dstOffset;

            while (i < count)
            {
                int run = RunLength(src, offset + i, count - i);
                if (run >= 3)
                {
                    dst[d++] = (byte)(1 - run); // -(run - 1)
                    dst[d++] = src[offset + i];
                    i += run;
                    continue;
                }

                // リテラル: 次に 3 バイト以上のランが始まる位置 (または 128 バイト) まで
                int start = i;
                int literal = 0;
                while (i < count && literal < MaxPacket)
                {
                    if (i + 2 < count
                        && src[offset + i] == src[offset + i + 1]
                        && src[offset + i] == src[offset + i + 2])
                    {
                        break;
                    }
                    i++;
                    literal++;
                }

                dst[d++] = (byte)(literal - 1);
                Buffer.BlockCopy(src, offset + start, dst, d, literal);
                d += literal;
            }

            return d - dstOffset;
        }

        private static int RunLength(byte[] src, int start, int remaining)
        {
            byte value = src[start];
            int max = Math.Min(remaining, MaxPacket);
            int run = 1;
            while (run < max && src[start + run] == value) run++;
            return run;
        }
    }
}
