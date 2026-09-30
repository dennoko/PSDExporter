using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>Texture2D のピクセルを左上原点の <see cref="RgbaImage"/> として読み出す。</summary>
    public interface ITexturePixelReader
    {
        bool CanRead(Texture2D texture);
        RgbaImage Read(Texture2D texture);
    }
}
