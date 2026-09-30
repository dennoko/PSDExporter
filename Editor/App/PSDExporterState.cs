using System;
using DennokoWorks.Tool.PSDExporter.Model;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.App
{
    /// <summary>
    /// <see cref="ExportProject"/> を保持する ScriptableObject。Undo とドメインリロード越しの
    /// 状態保持のためだけに存在し、アセットとしては保存しない (HideFlags.DontSave)。
    /// </summary>
    public sealed class PSDExporterState : ScriptableObject
    {
        public ExportProject Project = new ExportProject();

        [NonSerialized] private int _revision;

        /// <summary>変更ごとに加算される。プレビュー等のキャッシュ無効化に使う。</summary>
        public int Revision => _revision;

        public event Action Changed;

        public static PSDExporterState CreateTransient()
        {
            var state = CreateInstance<PSDExporterState>();
            state.hideFlags = HideFlags.DontSave;
            return state;
        }

        public void NotifyChanged()
        {
            _revision++;
            Changed?.Invoke();
        }
    }
}
