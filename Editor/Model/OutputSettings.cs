using System;

namespace DennokoWorks.Tool.PSDExporter.Model
{
    public enum OverwritePolicy
    {
        /// <summary>既存ファイルがあれば確認する。</summary>
        Ask = 0,
        Overwrite = 1,

        /// <summary>"_1", "_2" ... を付けて別名で保存する。</summary>
        AutoRename = 2,
    }

    [Serializable]
    public sealed class OutputSettings
    {
        /// <summary>"Assets/..." のプロジェクト相対パス、またはプロジェクト外の絶対パス。</summary>
        public string Folder = "Assets";

        public string FilePrefix = "Texture";
        public OverwritePolicy Overwrite = OverwritePolicy.Ask;

        /// <summary>出力先が Assets 配下ならインポートして Project ウィンドウに表示する。</summary>
        public bool ImportIntoAssetDatabase = true;
    }
}
