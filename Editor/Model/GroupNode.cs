using System;
using System.Collections.Generic;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Model
{
    [Serializable]
    public sealed class GroupNode : LayerTreeNode
    {
        public bool Expanded = true;

        /// <summary>子ノード。「上 → 下」の順。</summary>
        [SerializeReference]
        public List<LayerTreeNode> Children = new List<LayerTreeNode>();
    }
}
