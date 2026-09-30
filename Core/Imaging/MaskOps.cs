using System;
using System.Collections.Generic;

namespace DennokoWorks.Tool.PSDExporter.Core.Imaging
{
    /// <summary>マスク同士・マスクと画像の演算。</summary>
    public static class MaskOps
    {
        /// <summary>全マスクの合併 (画素ごとの最大値)。マスクが無い場合は全 0。</summary>
        public static MaskImage Union(IReadOnlyList<MaskImage> masks, int width, int height)
        {
            if (masks == null) throw new ArgumentNullException(nameof(masks));

            var result = new MaskImage(width, height);
            var dst = result.Values;

            foreach (var mask in masks)
            {
                EnsureSize(mask, width, height);
                var src = mask.Values;
                for (int i = 0; i < dst.Length; i++)
                {
                    if (src[i] > dst[i]) dst[i] = src[i];
                }
            }

            return result;
        }

        /// <summary>反転した新しいマスクを返す。</summary>
        public static MaskImage Invert(MaskImage mask)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));

            var result = new MaskImage(mask.Width, mask.Height);
            var src = mask.Values;
            var dst = result.Values;
            for (int i = 0; i < dst.Length; i++) dst[i] = (byte)(255 - src[i]);
            return result;
        }

        /// <summary>画像のアルファにマスク値を乗算する (インプレース)。</summary>
        public static void MultiplyAlpha(RgbaImage image, MaskImage mask)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            EnsureSize(mask, image.Width, image.Height);

            var px = image.Pixels;
            var m = mask.Values;
            for (int i = 0, p = 3; i < m.Length; i++, p += 4)
            {
                px[p] = (byte)((px[p] * m[i] + 127) / 255);
            }
        }

        internal static void EnsureSize(MaskImage mask, int width, int height)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            if (!mask.SizeEquals(width, height))
            {
                throw new ArgumentException(
                    $"Mask size {mask.Width}x{mask.Height} does not match {width}x{height}.", nameof(mask));
            }
        }
    }
}
