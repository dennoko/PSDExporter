using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    public enum SlotExportStatus
    {
        Succeeded,
        Failed,
        Skipped,
    }

    public sealed class SlotExportResult
    {
        public int SlotIndex { get; }
        public string Suffix { get; }
        public SlotExportStatus Status { get; }
        public string OutputPath { get; }
        public string ErrorMessage { get; }
        public Exception Exception { get; }

        public SlotExportResult(int slotIndex, string suffix, SlotExportStatus status,
            string outputPath = null, string errorMessage = null, Exception exception = null)
        {
            SlotIndex = slotIndex;
            Suffix = suffix;
            Status = status;
            OutputPath = outputPath;
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }

    /// <summary>エクスポート結果。処理中に Debug.Log を乱発せず、ここに集約して最後に出力する。</summary>
    public sealed class ExportReport
    {
        public List<SlotExportResult> Slots { get; } = new List<SlotExportResult>();
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>スロットに紐付かない致命的なエラー (検証エラー・準備失敗など)。</summary>
        public List<string> Errors { get; } = new List<string>();

        public bool Cancelled { get; set; }
        public TimeSpan Elapsed { get; set; }

        public int SucceededCount => Slots.Count(s => s.Status == SlotExportStatus.Succeeded);
        public int FailedCount => Slots.Count(s => s.Status == SlotExportStatus.Failed);

        public bool Succeeded => !Cancelled && Errors.Count == 0 && FailedCount == 0 && SucceededCount > 0;

        public void AddSuccess(int slotIndex, string suffix, string outputPath)
            => Slots.Add(new SlotExportResult(slotIndex, suffix, SlotExportStatus.Succeeded, outputPath));

        public void AddFailure(int slotIndex, string suffix, string message, Exception exception = null)
            => Slots.Add(new SlotExportResult(slotIndex, suffix, SlotExportStatus.Failed, null, message, exception));

        public void AddSkipped(int slotIndex, string suffix, string reason)
            => Slots.Add(new SlotExportResult(slotIndex, suffix, SlotExportStatus.Skipped, null, reason));

        public string Summary
        {
            get
            {
                if (Cancelled) return $"キャンセルされました (成功 {SucceededCount} 件)。";
                if (Errors.Count > 0) return "エクスポートできませんでした。";
                return $"成功 {SucceededCount} 件 / 失敗 {FailedCount} 件 ({Elapsed.TotalSeconds:0.0} 秒)";
            }
        }

        public void LogToConsole()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[PSD Exporter] {Summary}");

            foreach (var error in Errors) sb.AppendLine($"  エラー: {error}");
            foreach (var slot in Slots)
            {
                switch (slot.Status)
                {
                    case SlotExportStatus.Succeeded:
                        sb.AppendLine($"  ✔ {slot.Suffix}: {slot.OutputPath}");
                        break;
                    case SlotExportStatus.Failed:
                        sb.AppendLine($"  ✖ {slot.Suffix}: {slot.ErrorMessage}");
                        break;
                    case SlotExportStatus.Skipped:
                        sb.AppendLine($"  - {slot.Suffix}: {slot.ErrorMessage}");
                        break;
                }
            }
            foreach (var warning in Warnings) sb.AppendLine($"  注意: {warning}");

            if (Errors.Count > 0 || FailedCount > 0) Debug.LogError(sb.ToString());
            else if (Warnings.Count > 0 || Cancelled) Debug.LogWarning(sb.ToString());
            else Debug.Log(sb.ToString());

            foreach (var slot in Slots)
            {
                if (slot.Exception != null && !(slot.Exception is ExportException)) Debug.LogException(slot.Exception);
            }
        }
    }
}
