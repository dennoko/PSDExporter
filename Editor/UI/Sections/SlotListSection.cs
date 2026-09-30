using DennokoWorks.Tool.PSDExporter.App;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.UI.Sections
{
    /// <summary>入力テクスチャスロットの一覧 (暫定 UI)。</summary>
    internal sealed class SlotListSection
    {
        public void Draw(UIContext ctx)
        {
            var c = ctx.Controller;
            var slots = c.Project.Slots;

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(UIText.SlotsHeader, EditorStyles.boldLabel);

            for (int i = 0; i < slots.Count; i++)
            {
                DrawSlot(ctx, i);
            }

            if (UIWidgets.TextureDropArea(UIText.SlotsDrop, out var dropped))
            {
                ctx.Defer(() => c.AddSlots(dropped));
            }

            if (GUILayout.Button(UIText.AddSlot))
            {
                ctx.Defer(() => c.AddSlot());
            }
        }

        private static void DrawSlot(UIContext ctx, int index)
        {
            var c = ctx.Controller;
            var slot = c.Project.Slots[index];
            int count = c.Project.Slots.Count;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var enabled = GUILayout.Toggle(slot.Enabled, new GUIContent("", UIText.SlotEnabledTooltip), GUILayout.Width(16));
                if (EditorGUI.EndChangeCheck()) c.Modify("Toggle Slot", p => p.Slots[index].Enabled = enabled);

                EditorGUI.BeginChangeCheck();
                var texture = (Texture2D)EditorGUILayout.ObjectField(slot.Texture, typeof(Texture2D), false, GUILayout.MinWidth(120));
                if (EditorGUI.EndChangeCheck()) c.Modify("Change Slot Texture", p => p.Slots[index].Texture = texture);

                EditorGUI.BeginChangeCheck();
                var suffix = EditorGUILayout.TextField(new GUIContent("", UIText.SuffixTooltip), slot.Suffix, GUILayout.MinWidth(80));
                if (EditorGUI.EndChangeCheck()) c.Modify("Change Slot Suffix", p => p.Slots[index].Suffix = suffix);

                if (GUILayout.Button(new GUIContent("▼", UIText.SuffixMenuTooltip), UIWidgets.MiniButton))
                {
                    ShowSuffixMenu(c, index);
                }

                UIWidgets.ValidationBadge(ctx.FirstMessageForSlot(index));

                if (UIWidgets.IconButton("↑", UIText.MoveUpTooltip, index > 0))
                    ctx.Defer(() => c.MoveSlot(index, index - 1));
                if (UIWidgets.IconButton("↓", UIText.MoveDownTooltip, index < count - 1))
                    ctx.Defer(() => c.MoveSlot(index, index + 1));
                if (UIWidgets.IconButton("×", UIText.RemoveTooltip))
                    ctx.Defer(() => c.RemoveSlot(index));
            }
        }

        private static void ShowSuffixMenu(PSDExporterController c, int index)
        {
            var menu = new GenericMenu();
            foreach (var suffix in SlotSuffixSuggester.CommonSuffixes)
            {
                var value = suffix;
                menu.AddItem(new GUIContent(value), c.Project.Slots[index].Suffix == value,
                    () => c.Modify("Change Slot Suffix", p => p.Slots[index].Suffix = value));
            }
            menu.ShowAsContext();
        }
    }
}
