using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>
    /// インポート設定を一時変更する前にスナップショットを Library/ に保存しておき、
    /// Unity がクラッシュした場合でも次回起動時に復元できるようにする。
    /// </summary>
    public sealed class ImporterRestoreJournal
    {
        [Serializable]
        private sealed class JournalData
        {
            public List<ImporterSettingsSnapshot> Entries = new List<ImporterSettingsSnapshot>();
        }

        public string FilePath { get; }

        public ImporterRestoreJournal(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public static ImporterRestoreJournal CreateDefault()
        {
            var projectRoot = Path.GetDirectoryName(UnityEngine.Application.dataPath) ?? ".";
            return new ImporterRestoreJournal(
                Path.Combine(projectRoot, "Library", "dennokoworks", "PSDExporter", "restore_journal.json"));
        }

        public bool Exists => File.Exists(FilePath);

        public void Save(IEnumerable<ImporterSettingsSnapshot> snapshots)
        {
            var data = new JournalData();
            data.Entries.AddRange(snapshots);

            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }

        public List<ImporterSettingsSnapshot> Load()
        {
            if (!Exists) return new List<ImporterSettingsSnapshot>();

            var data = JsonUtility.FromJson<JournalData>(File.ReadAllText(FilePath));
            return data?.Entries ?? new List<ImporterSettingsSnapshot>();
        }

        public void Delete()
        {
            if (Exists) File.Delete(FilePath);
        }
    }
}
