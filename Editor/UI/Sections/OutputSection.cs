using System.IO;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.Output;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.UI.Sections
{
    /// <summary>保存先の設定 (暫定 UI)。</summary>
    internal sealed class OutputSection
    {
        public void Draw(UIContext ctx)
        {
            var c = ctx.Controller;
            var output = c.Project.Output;

            bool expanded = UIWidgets.SectionHeader(UIText.OutputHeader, ctx.ShowOutput);
            if (expanded != ctx.ShowOutput) ctx.Defer(() => ctx.ShowOutput = expanded);

            if (ctx.ShowOutput)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var folder = EditorGUILayout.TextField(UIText.OutputFolder, output.Folder);
                    if (EditorGUI.EndChangeCheck()) c.Modify("Change Output Folder", p => p.Output.Folder = folder);

                    if (GUILayout.Button(UIText.BrowseFolder, GUILayout.Width(28)))
                    {
                        ctx.Defer(() => BrowseFolder(ctx));
                    }
                }

                EditorGUI.BeginChangeCheck();
                var prefix = EditorGUILayout.TextField(UIText.FilePrefix, output.FilePrefix);
                if (EditorGUI.EndChangeCheck()) c.Modify("Change File Prefix", p => p.Output.FilePrefix = prefix);

                EditorGUI.BeginChangeCheck();
                var overwrite = (OverwritePolicy)EditorGUILayout.Popup(UIText.Overwrite, (int)output.Overwrite, UIText.OverwriteLabels);
                if (EditorGUI.EndChangeCheck()) c.Modify("Change Overwrite Policy", p => p.Output.Overwrite = overwrite);

                EditorGUI.BeginChangeCheck();
                var import = EditorGUILayout.Toggle(UIText.ImportIntoAssetDatabase, output.ImportIntoAssetDatabase);
                if (EditorGUI.EndChangeCheck()) c.Modify("Toggle Import", p => p.Output.ImportIntoAssetDatabase = import);

                DrawPlannedOutputs(ctx);
            }

            UIWidgets.EndSection();
        }

        private static void DrawPlannedOutputs(UIContext ctx)
        {
            var outputs = ctx.PlannedOutputs;
            if (outputs == null || outputs.Count == 0) return;

            EditorGUILayout.LabelField(UIText.OutputFiles, EditorStyles.miniLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var output in outputs)
                {
                    var name = Path.GetFileName(output.FullPath);
                    if (output.Exists) name += " " + UIText.WillOverwrite;
                    EditorGUILayout.LabelField(name, EditorStyles.miniLabel);
                }
            }
        }

        private static void BrowseFolder(UIContext ctx)
        {
            var c = ctx.Controller;
            string current;
            try
            {
                current = OutputPathResolver.ToFullPath(c.Project.Output.Folder);
            }
            catch (System.Exception)
            {
                current = OutputPathResolver.ProjectRoot;
            }

            var selected = EditorUtility.OpenFolderPanel(UIText.SelectFolderTitle, current, "");
            if (string.IsNullOrEmpty(selected)) return;

            var display = OutputPathResolver.ToDisplayFolder(selected);
            c.Modify("Change Output Folder", p => p.Output.Folder = display);
        }
    }
}
