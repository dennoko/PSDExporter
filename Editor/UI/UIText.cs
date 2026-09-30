namespace DennokoWorks.Tool.PSDExporter.UI
{
    /// <summary>UI の表示文字列。将来のローカライズや UI 差し替えに備えて 1 か所に集約する。</summary>
    internal static class UIText
    {
        public const string MenuPath = "dennokoworks/PSD Exporter";
        public const string WindowTitle = "PSD Exporter";
        public const string ProgressTitle = "PSD Exporter - エクスポート中";

        // スロット
        public const string SlotsHeader = "入力テクスチャ";
        public const string SlotsDrop = "テクスチャをここにドロップしてスロット追加";
        public const string AddSlot = "+ スロット追加";
        public const string SlotEnabledTooltip = "このスロットを出力する";
        public const string SuffixTooltip = "出力ファイル名のサフィックス ([接頭辞]_[サフィックス].psd)";
        public const string SuffixMenuTooltip = "よく使うサフィックスから選ぶ";

        // レイヤー
        public const string LayersHeader = "レイヤー構成 (上が前面)";
        public const string LayersDrop = "マスクテクスチャをここにドロップしてレイヤー追加";
        public const string AddLayer = "+ レイヤー";
        public const string AddGroup = "+ グループ";
        public const string LayersEmpty = "レイヤーがありません。マスクテクスチャをドロップするか「+ レイヤー」で追加してください。";
        public const string IncludeTooltip = "エクスポートに含める";
        public const string VisibleTooltip = "PSD 上で表示する";
        public const string MoveUpTooltip = "上へ";
        public const string MoveDownTooltip = "下へ";
        public const string OutdentTooltip = "グループから出す";
        public const string IndentTooltip = "上のグループに入れる";
        public const string RemoveTooltip = "削除";

        public const string DetailHeader = "選択中の項目";
        public const string Name = "名前";
        public const string Opacity = "不透明度";
        public const string Include = "エクスポートに含める";
        public const string Visible = "表示";
        public const string Mask = "マスク";
        public const string MaskChannel = "マスク成分";
        public const string InvertMask = "マスクを反転";
        public const string Expanded = "PSD 上で展開";

        public static readonly string[] MaskChannelLabels = { "輝度 (白黒)", "R", "G", "B", "アルファ" };

        // オプション
        public const string OptionsHeader = "出力オプション";
        public const string CutoutMode = "切り抜き方式";
        public static readonly string[] CutoutModeLabels = { "透過ピクセル切り抜き", "レイヤーマスク保持" };
        public const string IncludeBackground = "元画像の背景レイヤー";
        public const string IncludeUnassigned = "未分類の差分レイヤー";
        public const string LayerNameLabel = "レイヤー名";
        public const string TrimLayerBounds = "透明領域をトリム (サイズ削減)";
        public const string ReadQuality = "読み込み品質";
        public static readonly string[] ReadQualityLabels = { "元ファイル品質 (推奨)", "インポート結果のまま" };

        // 出力
        public const string OutputHeader = "保存先";
        public const string OutputFolder = "出力フォルダ";
        public const string BrowseFolder = "...";
        public const string SelectFolderTitle = "出力フォルダを選択";
        public const string FilePrefix = "ファイル接頭辞";
        public const string Overwrite = "既存ファイル";
        public static readonly string[] OverwriteLabels = { "上書き前に確認", "上書き", "別名で保存" };
        public const string ImportIntoAssetDatabase = "Assets 内ならインポートする";
        public const string OutputFiles = "出力されるファイル:";
        public const string WillOverwrite = "(上書き)";

        // プレビュー
        public const string PreviewHeader = "プレビュー";
        public const string PreviewSlot = "対象スロット";
        public const string RefreshPreview = "プレビュー更新";
        public const string PreviewStale = "設定が変更されています。「プレビュー更新」で再生成してください。";
        public const string PreviewEmpty = "「プレビュー更新」を押すと合成結果を表示します。";

        // 実行
        public const string Export = "エクスポート";
        public const string OverwriteDialogTitle = "上書きの確認";
        public const string OverwriteDialogMessage = "以下のファイルは既に存在します。上書きしますか?\n\n";
        public const string OverwriteDialogOk = "上書き";
        public const string Cancel = "キャンセル";
        public const string LastResult = "前回の結果: ";
    }
}
