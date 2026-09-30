using System.Collections.Generic;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.App.Validation;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.UI
{
    /// <summary>暫定 UI で共通に使う小さな部品。</summary>
    internal static class UIWidgets
    {
        public static readonly Color SelectionColor = new Color(0.24f, 0.48f, 0.9f, 0.3f);

        private static GUIStyle _miniButton;

        public static GUIStyle MiniButton => _miniButton ?? (_miniButton = new GUIStyle(EditorStyles.miniButton)
        {
            padding = new RectOffset(2, 2, 1, 1),
            fixedWidth = 20,
        });

        public static bool IconButton(string text, string tooltip, bool enabled = true)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                return GUILayout.Button(new GUIContent(text, tooltip), MiniButton);
            }
        }

        public static bool SectionHeader(string title, bool expanded)
        {
            EditorGUILayout.Space(4);
            return EditorGUILayout.BeginFoldoutHeaderGroup(expanded, title);
        }

        public static void EndSection()
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>ドロップ領域を描き、Texture2D がドロップされたらそれらを返す。</summary>
        public static bool TextureDropArea(string label, out List<Texture2D> textures, float height = 32f)
        {
            textures = null;
            var rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUI.Box(rect, label, EditorStyles.helpBox);

            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) return false;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return false;

            var dropped = DragAndDrop.objectReferences.OfType<Texture2D>().ToList();
            if (dropped.Count == 0) return false;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                evt.Use();
                textures = dropped;
                return true;
            }

            evt.Use();
            return false;
        }

        public static MessageType ToMessageType(ValidationSeverity severity)
        {
            switch (severity)
            {
                case ValidationSeverity.Error: return MessageType.Error;
                case ValidationSeverity.Warning: return MessageType.Warning;
                default: return MessageType.Info;
            }
        }

        /// <summary>検証メッセージがあれば行末に小さなアイコンで示す。</summary>
        public static void ValidationBadge(ValidationMessage message)
        {
            if (message == null) return;

            var icon = message.Severity == ValidationSeverity.Error ? "console.erroricon.sml"
                : message.Severity == ValidationSeverity.Warning ? "console.warnicon.sml"
                : "console.infoicon.sml";
            var content = new GUIContent(EditorGUIUtility.IconContent(icon)) { tooltip = message.Text };
            GUILayout.Label(content, GUILayout.Width(18), GUILayout.Height(18));
        }
    }
}
