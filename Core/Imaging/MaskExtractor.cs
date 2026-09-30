using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Imaging
{
    /// <summary>RGBA 画像から指定成分を取り出してマスクを作る。</summary>
    public static class MaskExtractor
    {
        public static MaskImage Extract(RgbaImage image, MaskChannel channel, bool invert)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var src = image.Pixels;
            var mask = new MaskImage(image.Width, image.Height);
            var dst = mask.Values;

            for (int i = 0, p = 0; i < dst.Length; i++, p += 4)
            {
                int value;
                switch (channel)
                {
                    case MaskChannel.Luminance:
                        // ITU-R BT.601 近似 (整数演算) × アルファ
                        int lum = (src[p] * 77 + src[p + 1] * 150 + src[p + 2] * 29 + 128) >> 8;
                        value = (lum * src[p + 3] + 127) / 255;
                        break;
                    case MaskChannel.Red: value = src[p]; break;
                    case MaskChannel.Green: value = src[p + 1]; break;
                    case MaskChannel.Blue: value = src[p + 2]; break;
                    case MaskChannel.Alpha: value = src[p + 3]; break;
                    default: throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
                }

                dst[i] = (byte)(invert ? 255 - value : value);
            }

            return mask;
        }
    }
}
