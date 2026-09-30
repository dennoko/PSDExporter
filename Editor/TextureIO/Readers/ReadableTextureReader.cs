using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.TextureIO.Readers
{
    /// <summary>Read/Write が有効なテクスチャを GetPixels32 で読む。</summary>
    public sealed class ReadableTextureReader : ITexturePixelReader
    {
        public bool CanRead(Texture2D texture) => texture != null && texture.isReadable;

        public RgbaImage Read(Texture2D texture)
        {
            var pixels = texture.GetPixels32(0);
            return UnityImageConverter.FromColor32(pixels, texture.width, texture.height);
        }
    }
}
