using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.TextureIO.Readers;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using PackageSource = UnityEditor.PackageManager.PackageSource;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>
    /// 1 回のエクスポート (またはプレビュー生成) の間、対象テクスチャのインポート設定を一括で一時変更し、
    /// Dispose で必ず元に戻す。変更前にジャーナルを保存し、クラッシュ時も次回起動時に復元できる。
    /// </summary>
    public sealed class TextureAccessSession : ITextureSource
    {
        private readonly ImporterRestoreJournal _journal;
        private readonly List<ImporterSettingsSnapshot> _modified = new List<ImporterSettingsSnapshot>();
        private readonly HashSet<string> _immutablePaths = new HashSet<string>();
        private readonly List<string> _warnings = new List<string>();

        private readonly ITexturePixelReader _readableReader = new ReadableTextureReader();
        private readonly ITexturePixelReader _fallbackReader = new GpuReadbackReader();

        private bool _disposed;

        /// <summary>読み込み時に発生した注意事項 (フォールバック読み込み等)。</summary>
        public IReadOnlyList<string> Warnings => _warnings;

        public TextureAccessSession(IEnumerable<Texture2D> textures, ImporterOverridePolicy policy, ImporterRestoreJournal journal)
        {
            if (textures == null) throw new ArgumentNullException(nameof(textures));
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            _journal = journal ?? throw new ArgumentNullException(nameof(journal));

            // 前回の中断分が残っていれば、スナップショットを取る前に復元しておく
            ImporterRestoreOnLoad.RestorePending(_journal);

            try
            {
                ApplyOverrides(textures, policy);
            }
            catch
            {
                RestoreAll();
                throw;
            }
        }

        public static TextureAccessSession Open(IEnumerable<Texture2D> textures, SourceReadQuality quality)
        {
            return new TextureAccessSession(textures, new ImporterOverridePolicy(quality), ImporterRestoreJournal.CreateDefault());
        }

        public RgbaImage Read(Texture2D texture)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TextureAccessSession));
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            if (_readableReader.CanRead(texture))
            {
                try
                {
                    return _readableReader.Read(texture);
                }
                catch (Exception e) when (e is ArgumentException || e is UnityException)
                {
                    // 一部の圧縮形式 (Crunch 等) は GetPixels32 に対応しないため GPU 経由で読む
                }
            }

            var path = AssetDatabase.GetAssetPath(texture);
            _warnings.Add(_immutablePaths.Contains(path)
                ? $"「{texture.name}」は変更不可のパッケージ内にあるため、インポート済み (圧縮済みの可能性あり) のピクセルを使用しました。"
                : $"「{texture.name}」は GPU 経由で読み込みました (圧縮による劣化の可能性があります)。");

            return _fallbackReader.Read(texture);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            RestoreAll();
        }

        private void ApplyOverrides(IEnumerable<Texture2D> textures, ImporterOverridePolicy policy)
        {
            var platformName = GetActivePlatformName();
            var targets = new List<(TextureImporter importer, ImporterSettingsSnapshot snapshot)>();
            var visited = new HashSet<string>();

            // 復元に失敗したまま残っているエントリは「本来の設定」なので、再取得せずそれを使う
            var pending = new Dictionary<string, ImporterSettingsSnapshot>();
            foreach (var entry in _journal.Load()) pending[entry.AssetPath] = entry;

            foreach (var texture in textures)
            {
                if (texture == null) continue;

                var path = AssetDatabase.GetAssetPath(texture);
                if (string.IsNullOrEmpty(path) || !visited.Add(path)) continue;

                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                if (IsImmutable(path))
                {
                    _immutablePaths.Add(path);
                    continue;
                }

                if (pending.TryGetValue(path, out var original))
                {
                    targets.Add((importer, original));
                    continue;
                }

                var snapshot = ImporterSettingsSnapshot.Capture(path, importer, platformName);
                if (policy.RequiresChange(snapshot)) targets.Add((importer, snapshot));
            }

            if (targets.Count == 0) return;

            var journalEntries = new Dictionary<string, ImporterSettingsSnapshot>(pending);
            foreach (var target in targets) journalEntries[target.snapshot.AssetPath] = target.snapshot;
            _journal.Save(journalEntries.Values);

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var (importer, snapshot) in targets)
                {
                    // 適用途中で失敗しても復元対象に含めるよう、適用前に登録する
                    _modified.Add(snapshot);
                    policy.Apply(importer, platformName);
                    importer.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private void RestoreAll()
        {
            if (_modified.Count == 0)
            {
                _journal.Delete();
                return;
            }

            var failed = TextureImporterRestorer.RestoreAll(_modified);
            var restoredPaths = new HashSet<string>();
            foreach (var snapshot in _modified) restoredPaths.Add(snapshot.AssetPath);
            _modified.Clear();

            // 失敗があればジャーナルを残し、次回起動時に再試行させる
            if (failed > 0) return;

            var remaining = _journal.Load().FindAll(e => !restoredPaths.Contains(e.AssetPath));
            if (remaining.Count == 0) _journal.Delete();
            else _journal.Save(remaining);
        }

        private static string GetActivePlatformName()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            return NamedBuildTarget.FromBuildTargetGroup(group).TargetName;
        }

        private static bool IsImmutable(string assetPath)
        {
            if (!assetPath.StartsWith("Packages/", StringComparison.Ordinal)) return false;

            var package = PackageInfo.FindForAssetPath(assetPath);
            if (package == null) return false;

            return package.source != PackageSource.Embedded && package.source != PackageSource.Local;
        }
    }
}
