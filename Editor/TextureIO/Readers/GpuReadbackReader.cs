using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace DennokoWorks.Tool.PSDExporter.TextureIO.Readers
{
    /// <summary>
    /// インポート設定を変更できないテクスチャ (非アセット・不変パッケージ内など) 用のフォールバック。
    /// GPU 上のテクスチャを RenderTexture 経由で読み戻す。圧縮済みの場合は劣化したピクセルになる。
    /// </summary>
    public sealed class GpuReadbackReader : ITexturePixelReader
    {
        public bool CanRead(Texture2D texture) => texture != null;

        public RgbaImage Read(Texture2D texture)
        {
            int w = texture.width;
            int h = texture.height;

            // ソースの色空間に合わせ、サンプリング時の変換と書き込み時の変換が打ち消し合うようにする
            bool isSrgb = GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
            var readWrite = isSrgb ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

            var previous = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, readWrite);
            Texture2D readback = null;
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;

                readback = new Texture2D(w, h, TextureFormat.RGBA32, false, !isSrgb)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                readback.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);

                return UnityImageConverter.FromColor32(readback.GetPixels32(0), w, h);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                if (readback != null) Object.DestroyImmediate(readback);
            }
        }
    }
}
