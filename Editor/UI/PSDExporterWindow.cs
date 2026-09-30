using System.Linq;
using DennokoWorks.Tool.PSDExporter.App;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.Pipeline;
using DennokoWorks.Tool.PSDExporter.Preview;
using DennokoWorks.Tool.PSDExporter.UI.Sections;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.UI
{
    /// <summary>
    /// PSD Exporter のメインウィンドウ (暫定 IMGUI 実装)。
    /// セクションを並べて描画するだけで、ロジックは <see cref="PSDExporterController"/> が持つ。
    /// </summary>
    public sealed class PSDExporterWindow : EditorWindow
    {
        [SerializeField] private PSDExporterState _state;
        [SerializeField] private UIContext _ui = new UIContext();
        [SerializeField] private Vector2 _scroll;

        private PSDExporterController _controller;
        private PreviewService _preview;
        private ExportReport _lastReport;

        private readonly SlotListSection _slotSection = new SlotListSection();
        private readonly LayerTreeSection _layerSection = new LayerTreeSection();
        private readonly OptionsSection _optionsSection = new OptionsSection();
        private readonly OutputSection _outputSection = new OutputSection();
        private readonly PreviewSection _previewSection = new PreviewSection();

        [MenuItem(UIText.MenuPath)]
        private static void Open()
        {
            var window = GetWindow<PSDExporterWindow>();
            window.titleContent = new GUIContent(UIText.WindowTitle);
            window.minSize = new Vector2(520, 480);
            window.Show();
        }

        private void OnEnable()
        {
            if (_state == null) _state = PSDExporterState.CreateTransient();
            if (_ui == null) _ui = new UIContext();

            _controller = PSDExporterController.CreateDefault(_state);
            _preview = PreviewService.CreateDefault();

            _controller.Changed += Repaint;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            if (_controller != null) _controller.Changed -= Repaint;

            _preview?.Dispose();
            _preview = null;
        }

        private void OnDestroy()
        {
            if (_state != null)
            {
                Undo.ClearUndo(_state);
                DestroyImmediate(_state);
                _state = null;
            }
        }

        private void OnUndoRedo()
        {
            _controller?.NotifyExternalChange();
            Repaint();
        }

        private void OnGUI()
        {
            if (_controller == null) return;

            _ui.BeginFrame(_controller, _preview, Repaint);

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;

                _slotSection.Draw(_ui);
                _layerSection.Draw(_ui);
                _optionsSection.Draw(_ui);
                _outputSection.Draw(_ui);
                _previewSection.Draw(_ui);
            }

            DrawFooter();

            _ui.FlushDeferred();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(4);

            foreach (var message in _ui.Validation.Where(m => m.NodeId == null && m.SlotIndex < 0))
            {
                EditorGUILayout.HelpBox(message.Text, UIWidgets.ToMessageType(message.Severity));
            }

            int itemMessages = _ui.Validation.Count(m => m.NodeId != null || m.SlotIndex >= 0);
            if (itemMessages > 0)
            {
                var worst = _ui.Validation.Where(m => m.NodeId != null || m.SlotIndex >= 0).Max(m => m.Severity);
                EditorGUILayout.HelpBox($"スロット/レイヤーに {itemMessages} 件の指摘があります (アイコンにカーソルを合わせると詳細を表示)。",
                    UIWidgets.ToMessageType(worst));
            }

            if (_lastReport != null)
            {
                var type = _lastReport.Succeeded ? MessageType.Info
                    : _lastReport.Cancelled ? MessageType.Warning
                    : MessageType.Error;
                EditorGUILayout.HelpBox(UIText.LastResult + _lastReport.Summary, type);
            }

            using (new EditorGUI.DisabledScope(_ui.HasErrors))
            {
                if (GUILayout.Button(UIText.Export, GUILayout.Height(32)))
                {
                    // プログレスバーやダイアログを OnGUI の外で扱うため遅延実行する
                    EditorApplication.delayCall += RunExport;
                }
            }
        }

        private void RunExport()
        {
            if (_controller == null) return;

            bool confirmed = true;
            if (_controller.Project.Output.Overwrite == OverwritePolicy.Ask)
            {
                var existing = _controller.PlanOutputs().Where(o => o.Exists).Select(o => o.FullPath).ToList();
                if (existing.Count > 0)
                {
                    confirmed = EditorUtility.DisplayDialog(UIText.OverwriteDialogTitle,
                        UIText.OverwriteDialogMessage + string.Join("\n", existing),
                        UIText.OverwriteDialogOk, UIText.Cancel);
                    if (!confirmed) return;
                }
            }

            using (var progress = new EditorProgressReporter(UIText.ProgressTitle))
            {
                _lastReport = _controller.Export(progress, confirmed);
            }

            _lastReport.LogToConsole();
            PingFirstOutput(_lastReport);
            Repaint();
        }

        private static void PingFirstOutput(ExportReport report)
        {
            var first = report.Slots.FirstOrDefault(s => s.Status == SlotExportStatus.Succeeded);
            if (first == null) return;

            var assetPath = Output.OutputPathResolver.ToAssetPath(first.OutputPath);
            if (assetPath == null) return;

            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null) EditorGUIUtility.PingObject(asset);
        }
    }
}
