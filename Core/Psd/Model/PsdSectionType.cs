namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    /// <summary>レイヤーグループ区切り (Additional Layer Info 'lsct') の種別。</summary>
    public enum PsdSectionType
    {
        /// <summary>通常レイヤー ('lsct' を書き込まない)。</summary>
        None = 0,
        OpenFolder = 1,
        ClosedFolder = 2,
        /// <summary>グループ終端 ("&lt;/Layer group&gt;")。</summary>
        BoundingDivider = 3,
    }
}
