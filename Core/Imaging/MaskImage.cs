using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Imaging
{
    /// <summary>
    /// 1 チャンネル 8bit のマスクバッファ。0 = 非選択, 255 = 選択。左上原点・行優先。
    /// </summary>
    public sealed class MaskImage
    {
        public int Width { get; }
        public int Height { get; }
        public byte[] Values { get; }

        public MaskImage(int width, int height)
            : this(width, height, new byte[checked(width * height)])
        {
        }

        /// <summary>既存の配列の所有権を受け取って生成する (コピーしない)。</summary>
        public MaskImage(int width, int height, byte[] values)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length != checked(width * height))
                throw new ArgumentException("Mask buffer length does not match the image size.", nameof(values));

            Width = width;
            Height = height;
            Values = values;
        }

        public static MaskImage Filled(int width, int height, byte value)
        {
            var mask = new MaskImage(width, height);
            if (value != 0)
            {
                for (int i = 0; i < mask.Values.Length; i++) mask.Values[i] = value;
            }
            return mask;
        }

        public bool SizeEquals(int width, int height) => Width == width && Height == height;

        public bool IsAllZero()
        {
            var v = Values;
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] != 0) return false;
            }
            return true;
        }

        public MaskImage Clone() => new MaskImage(Width, Height, (byte[])Values.Clone());
    }
}
