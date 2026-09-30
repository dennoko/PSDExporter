using System;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    public enum PsdBlendMode
    {
        Normal,
        PassThrough,
        Multiply,
        Screen,
        Overlay,
        Darken,
        Lighten,
        LinearDodge,
    }

    public static class PsdBlendModeExtensions
    {
        /// <summary>PSD に書き込む 4 文字のブレンドモードキー。</summary>
        public static string ToKey(this PsdBlendMode mode)
        {
            switch (mode)
            {
                case PsdBlendMode.Normal: return "norm";
                case PsdBlendMode.PassThrough: return "pass";
                case PsdBlendMode.Multiply: return "mul ";
                case PsdBlendMode.Screen: return "scrn";
                case PsdBlendMode.Overlay: return "over";
                case PsdBlendMode.Darken: return "dark";
                case PsdBlendMode.Lighten: return "lite";
                case PsdBlendMode.LinearDodge: return "lddg";
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}
