using System;
using System.Collections.Generic;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.App;
using DennokoWorks.Tool.PSDExporter.App.Validation;
using DennokoWorks.Tool.PSDExporter.Output;
using DennokoWorks.Tool.PSDExporter.Preview;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.UI
{
    /// <summary>
    /// UI 固有の状態 (選択・折りたたみ等) と、1 フレーム分の描画コンテキスト。
    /// モデルには UI の状態を入れない。
    /// </summary>
    /// <remarks>
    /// IMGUI は Layout と Repaint で同じコントロール構成を要求するため、表示内容を左右する値
    /// (検証結果・出力予定など) は Layout イベント時にだけ再計算し、構造を変える操作は
    /// <see cref="Defer"/> でフレーム末尾に遅延させる。
    /// </remarks>
    [Serializable]
    internal sealed class UIContext
    {
        // --- ドメインリロード越しに保持する UI 状態
        public string SelectedNodeId;
        public int PreviewSlotIndex;
        public bool ShowOptions = true;
        public bool ShowOutput = true;
        public bool ShowPreview = true;

        // --- フレームごとに設定される参照・スナップショット
        [NonSerialized] public PSDExporterController Controller;
        [NonSerialized] public PreviewService Preview;
        [NonSerialized] public Action RequestRepaint;
        [NonSerialized] public List<ValidationMessage> Validation;
        [NonSerialized] public List<ResolvedOutput> PlannedOutputs;
        [NonSerialized] public bool PreviewStale;

        [NonSerialized] private List<Action> _deferred;

        public bool HasErrors => Validation != null && ProjectValidator.HasErrors(Validation);

        public void BeginFrame(PSDExporterController controller, PreviewService preview, Action requestRepaint)
        {
            Controller = controller;
            Preview = preview;
            RequestRepaint = requestRepaint;

            if (Event.current.type == EventType.Layout || Validation == null)
            {
                Validation = controller.Validate();
                PlannedOutputs = SafePlanOutputs(controller);
                PreviewStale = preview.GeneratedRevision >= 0 && preview.IsStale(controller.Revision, PreviewSlotIndex);
            }
        }

        /// <summary>構造や表示内容を変える操作をフレーム末尾まで遅延する。</summary>
        public void Defer(Action action)
        {
            if (_deferred == null) _deferred = new List<Action>();
            _deferred.Add(action);
        }

        public void FlushDeferred()
        {
            if (_deferred == null || _deferred.Count == 0) return;

            var actions = _deferred.ToList();
            _deferred.Clear();
            foreach (var action in actions) action();

            // 選択変更など、モデル変更通知を伴わない UI 状態の変化も反映させる
            RequestRepaint?.Invoke();
        }

        public void Select(string nodeId) => Defer(() => SelectedNodeId = nodeId);

        public ValidationMessage FirstMessageFor(string nodeId)
        {
            return Validation?.FirstOrDefault(m => m.NodeId == nodeId);
        }

        public ValidationMessage FirstMessageForSlot(int slotIndex)
        {
            return Validation?.FirstOrDefault(m => m.SlotIndex == slotIndex);
        }

        private static List<ResolvedOutput> SafePlanOutputs(PSDExporterController controller)
        {
            try
            {
                return controller.PlanOutputs();
            }
            catch (Exception)
            {
                // 不正なフォルダパス等。検証メッセージ側で表示される
                return new List<ResolvedOutput>();
            }
        }
    }
}
