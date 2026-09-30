using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Imaging
{
    /// <summary>
    /// 通常ブレンドの over 合成 (straight alpha)。マージ済み画像とプレビューの生成で共用する。
    /// </summary>
    public static class AlphaCompositor
    {
        /// <param name="dst">合成先 (インプレースで更新)。</param>
        /// <param name="src">重ねる画像。</param>
        /// <param name="mask">レイヤーマスク。null ならマスクなし。</param>
        /// <param name="opacity">レイヤー不透明度 0..1。</param>
        public static void CompositeOver(RgbaImage dst, RgbaImage src, MaskImage mask, float opacity)
        {
            if (dst == null) throw new ArgumentNullException(nameof(dst));
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (!src.SizeEquals(dst.Width, dst.Height))
                throw new ArgumentException("Source and destination sizes differ.", nameof(src));
            if (mask != null) MaskOps.EnsureSize(mask, dst.Width, dst.Height);

            int op = (int)Math.Round(Math.Max(0f, Math.Min(1f, opacity)) * 255f);
            if (op == 0) return;

            var d = dst.Pixels;
            var s = src.Pixels;
            var m = mask?.Values;
            int count = dst.PixelCount;

            for (int i = 0, p = 0; i < count; i++, p += 4)
            {
                int sa = s[p + 3];
                if (m != null) sa = (sa * m[i] + 127) / 255;
                if (op != 255) sa = (sa * op + 127) / 255;
                if (sa == 0) continue;

                if (sa == 255)
                {
                    d[p] = s[p];
                    d[p + 1] = s[p + 1];
                    d[p + 2] = s[p + 2];
                    d[p + 3] = 255;
                    continue;
                }

                int da = d[p + 3];
                int dw = (da * (255 - sa) + 127) / 255; // 下地の実効寄与
                int outA = sa + dw;
                int half = outA >> 1;

                d[p] = (byte)((s[p] * sa + d[p] * dw + half) / outA);
                d[p + 1] = (byte)((s[p + 1] * sa + d[p + 1] * dw + half) / outA);
                d[p + 2] = (byte)((s[p + 2] * sa + d[p + 2] * dw + half) / outA);
                d[p + 3] = (byte)outA;
            }
        }
    }
}
