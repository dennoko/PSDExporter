using System.Collections.Generic;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    /// <summary>
    /// PSD のレイヤーレコード 1 件。通常レイヤー・グループ本体・グループ終端のいずれも表す。
    /// </summary>
    public sealed class PsdLayerRecord
    {
        public const string GroupEndName = "</Layer group>";

        public string Name { get; set; } = "Layer";
        public PsdRect Bounds { get; set; }
        public byte Opacity { get; set; } = 255;
        public bool Visible { get; set; } = true;
        public PsdBlendMode BlendMode { get; set; } = PsdBlendMode.Normal;
        public PsdSectionType Section { get; set; } = PsdSectionType.None;

        /// <summary>チャンネル ID: -1 (A), 0 (R), 1 (G), 2 (B), -2 (User Mask)。</summary>
        public List<PsdChannelData> Channels { get; } = new List<PsdChannelData>();

        /// <summary>null ならレイヤーマスクなし。</summary>
        public PsdLayerMask Mask { get; set; }

        public bool IsSectionRecord => Section != PsdSectionType.None;
    }
}
