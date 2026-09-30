namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    /// <summary>PSD の矩形 (top, left, bottom, right)。bottom / right は排他的。</summary>
    public readonly struct PsdRect
    {
        public readonly int Top;
        public readonly int Left;
        public readonly int Bottom;
        public readonly int Right;

        public PsdRect(int top, int left, int bottom, int right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }

        public static PsdRect Empty => new PsdRect(0, 0, 0, 0);

        public static PsdRect FromSize(int width, int height) => new PsdRect(0, 0, height, width);

        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public bool IsEmpty => Width <= 0 || Height <= 0;
    }
}
