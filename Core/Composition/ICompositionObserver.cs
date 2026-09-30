namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    public readonly struct CompositionProgress
    {
        public readonly int Index;
        public readonly int Total;
        public readonly string Key;
        public readonly string Name;

        public CompositionProgress(int index, int total, string key, string name)
        {
            Index = index;
            Total = total;
            Key = key;
            Name = name;
        }
    }

    /// <summary>合成処理の進捗通知・キャンセル・プレビュー取得用のフック。</summary>
    public interface ICompositionObserver
    {
        bool IsCancellationRequested { get; }

        /// <param name="progress">進捗情報。</param>
        /// <param name="layer">ピクセルレイヤーなら生成結果、グループ区切りなら null。</param>
        void OnLayerComposed(CompositionProgress progress, ComposedLayer layer);

        void OnWarning(string message);
    }
}
