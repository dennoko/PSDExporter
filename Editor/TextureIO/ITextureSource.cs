using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>
    /// 読み込み準備済みのテクスチャ群からピクセルを取得する窓口。Dispose で後始末 (設定復元) を行う。
    /// Pipeline / Preview はこのインターフェースにのみ依存する。
    /// </summary>
    public interface ITextureSource : IDisposable
    {
        RgbaImage Read(Texture2D texture);

        IReadOnlyList<string> Warnings { get; }
    }
}
