using System;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Model
{
    /// <summary>レイヤーツリーのノード。[SerializeReference] で多態シリアライズされる。</summary>
    [Serializable]
    public abstract class LayerTreeNode
    {
        /// <summary>UI の選択状態やプレビューキャッシュのキー。</summary>
        public string Id = NewId();

        public string Name;

        /// <summary>false ならエクスポート対象外。</summary>
        public bool IncludeInExport = true;

        /// <summary>PSD 上の表示フラグ。</summary>
        public bool Visible = true;

        [Range(0f, 1f)]
        public float Opacity = 1f;

        public static string NewId() => Guid.NewGuid().ToString("N");
    }
}
