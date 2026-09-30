using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DennokoWorks.Tool.PSDExporter.Model;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Output
{
    /// <summary>スロット 1 つ分の出力先。</summary>
    public sealed class ResolvedOutput
    {
        public int SlotIndex { get; }
        public string FullPath { get; }

        /// <summary>出力先が Assets 配下なら "Assets/..." 形式のパス、それ以外は null。</summary>
        public string AssetPath { get; }

        /// <summary>解決時点で同名ファイルが存在していたか。</summary>
        public bool Exists { get; }

        /// <summary>書き込み後に AssetDatabase へインポートするか (AssetPath がある場合のみ有効)。</summary>
        public bool ImportAfterWrite { get; }

        public ResolvedOutput(int slotIndex, string fullPath, string assetPath, bool exists, bool importAfterWrite)
        {
            SlotIndex = slotIndex;
            FullPath = fullPath;
            AssetPath = assetPath;
            Exists = exists;
            ImportAfterWrite = importAfterWrite;
        }
    }

    /// <summary>出力ファイルパスの組み立て・サニタイズ・衝突解決。</summary>
    public static class OutputPathResolver
    {
        public const string Extension = ".psd";

        public static string ProjectRoot => Path.GetDirectoryName(UnityEngine.Application.dataPath);

        /// <summary>"Assets/..." 等のプロジェクト相対パスまたは絶対パスを絶対パスにする。</summary>
        public static string ToFullPath(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) folder = "Assets";
            var path = Path.IsPathRooted(folder) ? folder : Path.Combine(ProjectRoot, folder);
            return Path.GetFullPath(path);
        }

        /// <summary>絶対パスが Assets 配下なら "Assets/..." 形式に、そうでなければ null を返す。</summary>
        public static string ToAssetPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            var assetsRoot = Path.GetFullPath(UnityEngine.Application.dataPath);
            var full = Path.GetFullPath(fullPath);

            if (string.Equals(full, assetsRoot, StringComparison.OrdinalIgnoreCase)) return "Assets";
            if (!full.StartsWith(assetsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return null;

            return "Assets" + full.Substring(assetsRoot.Length).Replace('\\', '/');
        }

        /// <summary>フォルダ選択ダイアログの結果を、可能ならプロジェクト相対パスにする (UI 用)。</summary>
        public static string ToDisplayFolder(string absoluteFolder)
        {
            return ToAssetPath(absoluteFolder) ?? absoluteFolder;
        }

        public static string BuildFileName(string prefix, string suffix)
        {
            prefix = prefix?.Trim() ?? string.Empty;
            suffix = suffix?.Trim() ?? string.Empty;

            string stem;
            if (prefix.Length == 0) stem = suffix;
            else if (suffix.Length == 0) stem = prefix;
            else stem = prefix + "_" + suffix;

            if (stem.Length == 0) stem = "Texture";
            return Sanitize(stem) + Extension;
        }

        public static string Sanitize(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(fileName.Length);
            foreach (char c in fileName) sb.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
            return sb.ToString();
        }

        /// <summary>エクスポート対象の全スロットの出力先を決定する。</summary>
        public static List<ResolvedOutput> Resolve(ExportProject project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            var settings = project.Output;
            var folder = ToFullPath(settings.Folder);
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<ResolvedOutput>();

            foreach (var entry in project.EnumerateExportableSlots())
            {
                var fileName = BuildFileName(settings.FilePrefix, entry.Slot.Suffix);
                var path = Path.Combine(folder, fileName);

                if (settings.Overwrite == OverwritePolicy.AutoRename)
                {
                    path = MakeUnique(path, used);
                }

                used.Add(path);
                result.Add(new ResolvedOutput(entry.Index, path, ToAssetPath(path), File.Exists(path), settings.ImportIntoAssetDatabase));
            }

            return result;
        }

        private static string MakeUnique(string path, HashSet<string> used)
        {
            if (!File.Exists(path) && !used.Contains(path)) return path;

            var dir = Path.GetDirectoryName(path) ?? string.Empty;
            var stem = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);

            for (int i = 1; ; i++)
            {
                var candidate = Path.Combine(dir, $"{stem}_{i}{ext}");
                if (!File.Exists(candidate) && !used.Contains(candidate)) return candidate;
            }
        }
    }
}
