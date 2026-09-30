using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Imaging
{
    /// <summary>
    /// バイリニア補間によるリサイズ。ピクセル中心基準でサンプリングし、端はクランプする。
    /// 同サイズの場合は入力インスタンスをそのまま返す (コピーしない)。
    /// </summary>
    public static class BilinearResampler
    {
        public static RgbaImage Resize(RgbaImage src, int width, int height)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (src.SizeEquals(width, height)) return src;

            var pixels = Resize(src.Pixels, src.Width, src.Height, RgbaImage.Channels, width, height);
            return new RgbaImage(width, height, pixels);
        }

        public static MaskImage Resize(MaskImage src, int width, int height)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (src.SizeEquals(width, height)) return src;

            var values = Resize(src.Values, src.Width, src.Height, 1, width, height);
            return new MaskImage(width, height, values);
        }

        /// <summary>アスペクト比を保ったまま長辺が maxSize 以下になるサイズを返す。</summary>
        public static void FitWithin(int width, int height, int maxSize, out int fitWidth, out int fitHeight)
        {
            if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize));

            if (width <= maxSize && height <= maxSize)
            {
                fitWidth = width;
                fitHeight = height;
                return;
            }

            double scale = (double)maxSize / Math.Max(width, height);
            fitWidth = Math.Max(1, (int)Math.Round(width * scale));
            fitHeight = Math.Max(1, (int)Math.Round(height * scale));
        }

        private static byte[] Resize(byte[] src, int sw, int sh, int channels, int dw, int dh)
        {
            if (dw <= 0) throw new ArgumentOutOfRangeException(nameof(dw));
            if (dh <= 0) throw new ArgumentOutOfRangeException(nameof(dh));

            var dst = new byte[checked(dw * dh * channels)];

            // x 方向のサンプル位置と重みは全行で共通なので事前計算する
            var x0 = new int[dw];
            var x1 = new int[dw];
            var wx = new int[dw]; // 0..256 の固定小数点
            double scaleX = (double)sw / dw;
            for (int x = 0; x < dw; x++)
            {
                double fx = (x + 0.5) * scaleX - 0.5;
                if (fx < 0) fx = 0;
                int ix = (int)fx;
                if (ix > sw - 1) ix = sw - 1;
                x0[x] = ix * channels;
                x1[x] = Math.Min(ix + 1, sw - 1) * channels;
                wx[x] = (int)((fx - ix) * 256 + 0.5);
            }

            double scaleY = (double)sh / dh;
            int srcStride = sw * channels;
            int dstStride = dw * channels;

            for (int y = 0; y < dh; y++)
            {
                double fy = (y + 0.5) * scaleY - 0.5;
                if (fy < 0) fy = 0;
                int iy = (int)fy;
                if (iy > sh - 1) iy = sh - 1;
                int iy1 = Math.Min(iy + 1, sh - 1);
                int wy = (int)((fy - iy) * 256 + 0.5);

                int row0 = iy * srcStride;
                int row1 = iy1 * srcStride;
                int dRow = y * dstStride;

                for (int x = 0; x < dw; x++)
                {
                    int a = row0 + x0[x];
                    int b = row0 + x1[x];
                    int c = row1 + x0[x];
                    int d = row1 + x1[x];
                    int w = wx[x];
                    int dp = dRow + x * channels;

                    for (int ch = 0; ch < channels; ch++)
                    {
                        int top = src[a + ch] * (256 - w) + src[b + ch] * w;
                        int bottom = src[c + ch] * (256 - w) + src[d + ch] * w;
                        int value = (top * (256 - wy) + bottom * wy + 32768) >> 16;
                        dst[dp + ch] = (byte)value;
                    }
                }
            }

            return dst;
        }
    }
}
