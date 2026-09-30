using System;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Model
{
    [Serializable]
    public sealed class MaskLayerNode : LayerTreeNode
    {
        public Texture2D Mask;

        /// <summary>白黒マスクなら Luminance、アルファマスクなら Alpha。</summary>
        public MaskChannel Channel = MaskChannel.Luminance;

        public bool InvertMask;
    }
}
