using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.UI.Sections
{
    /// <summary>合成結果のプレビュー (暫定 UI)。再インポートを伴うため手動更新。</summary>
    internal sealed class PreviewSection
    {
        private const float MaxPreviewHeight = 320f;

        public void Draw(UIContext ctx)
        {
            var c = ctx.Controller;
            var preview = ctx.Preview;

            bool expanded = UIWidgets.SectionHeader(UIText.PreviewHeader, ctx.ShowPreview);
            if (expanded != ctx.ShowPreview) ctx.Defer(() => ctx.ShowPreview = expanded);

            if (ctx.ShowPreview)
            {
                var slots = c.Project.Slots;
                var labels = slots.Select((s, i) => $"{i + 1}: {(string.IsNullOrEmpty(s.Suffix) ? "(無名)" : s.Suffix)}").ToArray();
                int slotIndex = Mathf.Clamp(ctx.PreviewSlotIndex, 0, Mathf.Max(0, slots.Count - 1));

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(slots.Count == 0))
                    {
                        EditorGUI.BeginChangeCheck();
                        int selected = EditorGUILayout.Popup(UIText.PreviewSlot, slotIndex, labels);
                        if (EditorGUI.EndChangeCheck()) ctx.Defer(() => ctx.PreviewSlotIndex = selected);

                        if (GUILayout.Button(UIText.RefreshPreview, GUILayout.Width(110)))
                        {
                            ctx.PreviewSlotIndex = slotIndex;
                            // 再インポートを伴うため OnGUI の外で実行する
                            EditorApplication.delayCall += () =>
                            {
                                preview.Refresh(c.Project, slotIndex, c.Revision);
                                ctx.RequestRepaint?.Invoke();
                            };
                        }
                    }
                }

                if (!string.IsNullOrEmpty(preview.LastError))
                {
                    EditorGUILayout.HelpBox(preview.LastError, MessageType.Error);
                }
                foreach (var warning in preview.Warnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }

                if (ctx.PreviewStale)
                {
                    EditorGUILayout.HelpBox(UIText.PreviewStale, MessageType.Info);
                }

                DrawComposite(preview.Composite);
            }

            UIWidgets.EndSection();
        }

        private static void DrawComposite(Texture2D composite)
        {
            if (composite == null)
            {
                EditorGUILayout.LabelField(UIText.PreviewEmpty, EditorStyles.centeredGreyMiniLabel);
                return;
            }

            float aspect = (float)composite.width / composite.height;
            float width = EditorGUIUtility.currentViewWidth - 40f;
            float height = Mathf.Min(MaxPreviewHeight, width / aspect);

            var rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawTextureTransparent(rect, composite, ScaleMode.ScaleToFit);
            }
        }
    }
}
