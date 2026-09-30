using DennokoWorks.Tool.PSDExporter.Core.Composition;
using DennokoWorks.Tool.PSDExporter.Model;
using UnityEditor;

namespace DennokoWorks.Tool.PSDExporter.UI.Sections
{
    /// <summary>出力オプション (暫定 UI)。</summary>
    internal sealed class OptionsSection
    {
        public void Draw(UIContext ctx)
        {
            var c = ctx.Controller;
            var options = c.Project.Options;

            bool expanded = UIWidgets.SectionHeader(UIText.OptionsHeader, ctx.ShowOptions);
            if (expanded != ctx.ShowOptions) ctx.Defer(() => ctx.ShowOptions = expanded);

            if (ctx.ShowOptions)
            {
                EditorGUI.BeginChangeCheck();
                var mode = (CutoutMode)EditorGUILayout.Popup(UIText.CutoutMode, (int)options.CutoutMode, UIText.CutoutModeLabels);
                if (EditorGUI.EndChangeCheck()) c.Modify("Change Cutout Mode", p => p.Options.CutoutMode = mode);

                DrawAuxiliaryLayer(c, UIText.IncludeBackground, options.IncludeBackground, options.BackgroundLayerName,
                    (p, v) => p.Options.IncludeBackground = v, (p, v) => p.Options.BackgroundLayerName = v);

                DrawAuxiliaryLayer(c, UIText.IncludeUnassigned, options.IncludeUnassigned, options.UnassignedLayerName,
                    (p, v) => p.Options.IncludeUnassigned = v, (p, v) => p.Options.UnassignedLayerName = v);

                EditorGUI.BeginChangeCheck();
                var trim = EditorGUILayout.Toggle(UIText.TrimLayerBounds, options.TrimLayerBounds);
                if (EditorGUI.EndChangeCheck()) c.Modify("Toggle Trim", p => p.Options.TrimLayerBounds = trim);

                EditorGUI.BeginChangeCheck();
                var quality = (SourceReadQuality)EditorGUILayout.Popup(UIText.ReadQuality, (int)options.ReadQuality, UIText.ReadQualityLabels);
                if (EditorGUI.EndChangeCheck()) c.Modify("Change Read Quality", p => p.Options.ReadQuality = quality);
            }

            UIWidgets.EndSection();
        }

        private static void DrawAuxiliaryLayer(App.PSDExporterController c, string label, bool enabled, string layerName,
            System.Action<ExportProject, bool> setEnabled, System.Action<ExportProject, string> setName)
        {
            EditorGUI.BeginChangeCheck();
            var newEnabled = EditorGUILayout.Toggle(label, enabled);
            if (EditorGUI.EndChangeCheck()) c.Modify("Toggle " + label, p => setEnabled(p, newEnabled));

            // コントロール数を一定に保つため、無効時も非活性で表示する
            using (new EditorGUI.DisabledScope(!enabled))
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();
                var newName = EditorGUILayout.TextField(UIText.LayerNameLabel, layerName);
                if (EditorGUI.EndChangeCheck()) c.Modify("Rename " + label, p => setName(p, newName));
            }
        }
    }
}
