using System;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Model
{
    /// <summary>入力テクスチャ 1 枚。スロットごとに 1 つの PSD を出力する。</summary>
    [Serializable]
    public sealed class TextureSlot
    {
        public bool Enabled = true;
        public Texture2D Texture;

        /// <summary>出力ファイル名 "[接頭辞]_[Suffix].psd" のサフィックス。</summary>
        public string Suffix = "BaseColor";
    }
}
