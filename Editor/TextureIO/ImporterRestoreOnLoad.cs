using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>
    /// エディタ起動 (ドメインリロード) 時に未復元のジャーナルが残っていれば、インポート設定を復元する。
    /// </summary>
    [InitializeOnLoad]
    internal static class ImporterRestoreOnLoad
    {
        static ImporterRestoreOnLoad()
        {
            // 静的コンストラクタ内では AssetDatabase が使えない場合があるため遅延実行する
            EditorApplication.delayCall += () => RestorePending(ImporterRestoreJournal.CreateDefault());
        }

        /// <summary>未復元のジャーナルがあれば復元する。復元を行った場合 true。</summary>
        public static bool RestorePending(ImporterRestoreJournal journal)
        {
            if (journal == null || !journal.Exists) return false;

            List<ImporterSettingsSnapshot> snapshots;
            try
            {
                snapshots = journal.Load();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PSD Exporter] 復元ジャーナルの読み込みに失敗しました: {journal.FilePath}\n{e}");
                return false;
            }

            var failed = TextureImporterRestorer.RestoreAll(snapshots);
            if (failed == 0)
            {
                journal.Delete();
                Debug.Log($"[PSD Exporter] 前回中断されたエクスポートのテクスチャインポート設定を復元しました ({snapshots.Count} 件)。");
            }
            return true;
        }
    }

    /// <summary>スナップショット群を一括でインポーターに書き戻す。</summary>
    internal static class TextureImporterRestorer
    {
        /// <returns>復元に失敗した件数。</returns>
        public static int RestoreAll(IReadOnlyList<ImporterSettingsSnapshot> snapshots)
        {
            int failed = 0;
            if (snapshots == null || snapshots.Count == 0) return failed;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var snapshot in snapshots)
                {
                    try
                    {
                        var importer = AssetImporter.GetAtPath(snapshot.AssetPath) as TextureImporter;
                        if (importer == null) continue; // アセットが削除済み

                        snapshot.Restore(importer);
                        importer.SaveAndReimport();
                    }
                    catch (Exception e)
                    {
                        failed++;
                        Debug.LogError($"[PSD Exporter] インポート設定の復元に失敗しました: {snapshot.AssetPath}\n{e}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            return failed;
        }
    }
}
