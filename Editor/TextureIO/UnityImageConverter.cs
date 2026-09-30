using System;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>Unity の画像型 (左下原点) と Core の画像型 (左上原点) の相互変換。</summary>
    public static class UnityImageConverter
    {
        /// <summary>GetPixels32 の結果 (左下原点) を上下反転して RgbaImage にする。</summary>
        public static RgbaImage FromColor32(Color32[] pixels, int width, int height)
        {
            if (pixels == null) throw new ArgumentNullException(nameof(pixels));
            if (pixels.Length != width * height) throw new ArgumentException("Pixel count mismatch.", nameof(pixels));

            var image = new RgbaImage(width, height);
            var dst = image.Pixels;

            for (int y = 0; y < height; y++)
            {
                int srcRow = (height - 1 - y) * width;
                int dstRow = y * width * 4;
                for (int x = 0; x < width; x++)
                {
                    var c = pixels[srcRow + x];
                    int p = dstRow + x * 4;
                    dst[p] = c.r;
                    dst[p + 1] = c.g;
                    dst[p + 2] = c.b;
                    dst[p + 3] = c.a;
                }
            }

            return image;
        }

        /// <summary>表示用の Texture2D を生成する。呼び出し側が DestroyImmediate する責任を持つ。</summary>
        public static Texture2D ToTexture2D(RgbaImage image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            int w = image.Width;
            int h = image.Height;
            int stride = w * 4;
            var flipped = new byte[image.Pixels.Length];
            for (int y = 0; y < h; y++)
            {
                Buffer.BlockCopy(image.Pixels, y * stride, flipped, (h - 1 - y) * stride, stride);
            }

            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            texture.LoadRawTextureData(flipped);
            texture.Apply(false, false);
            return texture;
        }
    }
}
