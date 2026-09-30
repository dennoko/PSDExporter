using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Imaging
{
    /// <summary>
    /// RGBA8 のピクセルバッファ。左上原点・行優先・R,G,B,A の順で格納する。
    /// Unity (左下原点) との変換は Editor 側の TextureIO が担う。
    /// </summary>
    public sealed class RgbaImage
    {
        public const int Channels = 4;

        public int Width { get; }
        public int Height { get; }
        public byte[] Pixels { get; }

        public int PixelCount => Width * Height;

        public RgbaImage(int width, int height)
            : this(width, height, new byte[checked(width * height * Channels)])
        {
        }

        /// <summary>既存の配列の所有権を受け取って生成する (コピーしない)。</summary>
        public RgbaImage(int width, int height, byte[] pixels)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (pixels == null) throw new ArgumentNullException(nameof(pixels));
            if (pixels.Length != checked(width * height * Channels))
                throw new ArgumentException("Pixel buffer length does not match the image size.", nameof(pixels));

            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public bool SizeEquals(int width, int height) => Width == width && Height == height;

        public RgbaImage Clone() => new RgbaImage(Width, Height, (byte[])Pixels.Clone());
    }
}
