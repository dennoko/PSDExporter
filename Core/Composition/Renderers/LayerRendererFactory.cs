using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition.Renderers
{
    public static class LayerRendererFactory
    {
        public static ILayerRenderer Create(CutoutMode mode)
        {
            switch (mode)
            {
                case CutoutMode.TransparentCutout: return new TransparentCutoutRenderer();
                case CutoutMode.LayerMask: return new LayerMaskRenderer();
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
