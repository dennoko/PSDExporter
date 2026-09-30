using DennokoWorks.Tool.PSDExporter.App;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Model;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.UI.Sections
{
    /// <summary>
    /// レイヤー/グループのツリー (暫定 UI)。インデント付きの行リストと、選択項目の詳細パネル。
    /// 後で TreeView / UI Toolkit 版に差し替える前提。
    /// </summary>
    internal sealed class LayerTreeSection
    {
        private const float IndentWidth = 16f;
        private const float ThumbnailSize = 32f;

        private static GUIContent _folderContent;

        private static GUIContent FolderContent => _folderContent ??
            (_folderContent = new GUIContent(EditorGUIUtility.IconContent("Folder Icon")) { text = "Group" });

        private static GUIContent VisibilityContent(bool visible)
        {
            var icon = EditorGUIUtility.IconContent(visible ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff");
            return new GUIContent(icon) { tooltip = UIText.VisibleTooltip };
        }

        public void Draw(UIContext ctx)
        {
            var c = ctx.Controller;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(UIText.LayersHeader, EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(UIText.AddLayer))
                {
                    ctx.Defer(() =>
                    {
                        GetInsertTarget(ctx, out var parentId, out var index);
                        var layer = c.AddLayer(parentId, index);
                        if (layer != null) ctx.SelectedNodeId = layer.Id;
                    });
                }
                if (GUILayout.Button(UIText.AddGroup))
                {
                    ctx.Defer(() =>
                    {
                        GetInsertTarget(ctx, out var parentId, out var index);
                        var group = c.AddGroup(parentId, index);
                        if (group != null) ctx.SelectedNodeId = group.Id;
                    });
                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (c.Root.Children.Count == 0)
                {
                    EditorGUILayout.LabelField(UIText.LayersEmpty, EditorStyles.wordWrappedMiniLabel);
                }
                else
                {
                    DrawChildren(ctx, c.Root, 0);
                }
            }

            if (UIWidgets.TextureDropArea(UIText.LayersDrop, out var dropped))
            {
                ctx.Defer(() =>
                {
                    GetInsertTarget(ctx, out var parentId, out var index);
                    c.AddLayers(parentId, index, dropped);
                });
            }

            DrawDetail(ctx);
        }

        /// <summary>選択中がグループならその先頭、レイヤーならその直上、未選択ならルート先頭に追加する。</summary>
        private static void GetInsertTarget(UIContext ctx, out string parentId, out int index)
        {
            var c = ctx.Controller;
            var selected = c.FindNode(ctx.SelectedNodeId);

            if (selected is GroupNode group)
            {
                parentId = group.Id;
                index = 0;
                return;
            }

            if (selected != null && LayerTreeOperations.TryFindParent(c.Root, selected.Id, out var parent, out var selectedIndex))
            {
                parentId = parent.Id;
                index = selectedIndex;
                return;
            }

            parentId = null;
            index = 0;
        }

        private static void DrawChildren(UIContext ctx, GroupNode group, int depth)
        {
            // 描画中にリストが変わらないよう、構造変更は Defer 経由で行う
            for (int i = 0; i < group.Children.Count; i++)
            {
                var child = group.Children[i];
                if (child == null) continue;

                DrawRow(ctx, child, depth);

                if (child is GroupNode childGroup && childGroup.Expanded)
                {
                    DrawChildren(ctx, childGroup, depth + 1);
                }
            }
        }

        private static void DrawRow(UIContext ctx, LayerTreeNode node, int depth)
        {
            var c = ctx.Controller;
            var id = node.Id;
            bool selected = ctx.SelectedNodeId == id;

            var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(ThumbnailSize + 2));

            var evt = Event.current;
            if (evt.type == EventType.Repaint && selected)
            {
                EditorGUI.DrawRect(rowRect, UIWidgets.SelectionColor);
            }
            if (evt.type == EventType.MouseDown && rowRect.Contains(evt.mousePosition) && !selected)
            {
                // Use() せず、行内のコントロールにもイベントを渡す
                ctx.Select(id);
            }

            GUILayout.Space(depth * IndentWidth);

            if (node is GroupNode group)
            {
                bool expanded = GUILayout.Toggle(group.Expanded, GUIContent.none, EditorStyles.foldout, GUILayout.Width(14));
                if (expanded != group.Expanded)
                {
                    ctx.Defer(() => c.Modify("Toggle Group", _ => SetExpanded(c, id, expanded)));
                }
            }
            else
            {
                GUILayout.Space(14);
            }

            EditorGUI.BeginChangeCheck();
            bool include = GUILayout.Toggle(node.IncludeInExport, new GUIContent("", UIText.IncludeTooltip), GUILayout.Width(16));
            if (EditorGUI.EndChangeCheck()) c.Modify("Toggle Export", _ => ModifyNode(c, id, n => n.IncludeInExport = include));

            EditorGUI.BeginChangeCheck();
            bool visible = GUILayout.Toggle(node.Visible, VisibilityContent(node.Visible), EditorStyles.miniButton, GUILayout.Width(24));
            if (EditorGUI.EndChangeCheck()) c.Modify("Toggle Visibility", _ => ModifyNode(c, id, n => n.Visible = visible));

            DrawThumbnail(ctx, node);

            using (new EditorGUI.DisabledScope(!node.IncludeInExport))
            {
                EditorGUI.BeginChangeCheck();
                var style = node is GroupNode ? EditorStyles.boldLabel : EditorStyles.label;
                var name = EditorGUILayout.TextField(node.Name, GUILayout.MinWidth(80));
                if (EditorGUI.EndChangeCheck()) c.Modify("Rename Layer", _ => ModifyNode(c, id, n => n.Name = name));

                if (node is MaskLayerNode layer)
                {
                    EditorGUI.BeginChangeCheck();
                    var mask = (Texture2D)EditorGUILayout.ObjectField(layer.Mask, typeof(Texture2D), false, GUILayout.Width(110));
                    if (EditorGUI.EndChangeCheck()) c.Modify("Change Mask", _ => ModifyNode(c, id, n => ((MaskLayerNode)n).Mask = mask));
                }
                else
                {
                    GUILayout.Label(FolderContent, style, GUILayout.Width(110), GUILayout.Height(18));
                }
            }

            UIWidgets.ValidationBadge(ctx.FirstMessageFor(id));

            if (UIWidgets.IconButton("↑", UIText.MoveUpTooltip, c.CanMoveWithinParent(id, -1)))
                ctx.Defer(() => c.MoveUp(id));
            if (UIWidgets.IconButton("↓", UIText.MoveDownTooltip, c.CanMoveWithinParent(id, +1)))
                ctx.Defer(() => c.MoveDown(id));
            if (UIWidgets.IconButton("←", UIText.OutdentTooltip, c.CanOutdent(id)))
                ctx.Defer(() => c.Outdent(id));
            if (UIWidgets.IconButton("→", UIText.IndentTooltip, c.CanIndent(id)))
                ctx.Defer(() => c.Indent(id));
            if (UIWidgets.IconButton("×", UIText.RemoveTooltip))
            {
                ctx.Defer(() =>
                {
                    c.RemoveNode(id);
                    if (ctx.SelectedNodeId == id) ctx.SelectedNodeId = null;
                });
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawThumbnail(UIContext ctx, LayerTreeNode node)
        {
            var rect = GUILayoutUtility.GetRect(ThumbnailSize, ThumbnailSize, GUILayout.Width(ThumbnailSize), GUILayout.Height(ThumbnailSize));
            if (Event.current.type != EventType.Repaint) return;

            var thumbnail = node is MaskLayerNode ? ctx.Preview.GetThumbnail(node.Id) : null;
            if (thumbnail != null)
            {
                EditorGUI.DrawTextureTransparent(rect, thumbnail, ScaleMode.ScaleToFit);
            }
            else if (node is MaskLayerNode layer && layer.Mask != null)
            {
                var preview = AssetPreview.GetMiniThumbnail(layer.Mask);
                if (preview != null) GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
            }
        }

        private static void DrawDetail(UIContext ctx)
        {
            var c = ctx.Controller;
            var node = c.FindNode(ctx.SelectedNodeId);
            if (node == null || ReferenceEquals(node, c.Root)) return;

            var id = node.Id;

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(UIText.DetailHeader, EditorStyles.miniBoldLabel);

                EditorGUI.BeginChangeCheck();
                var name = EditorGUILayout.TextField(UIText.Name, node.Name);
                if (EditorGUI.EndChangeCheck()) c.Modify("Rename Layer", _ => ModifyNode(c, id, n => n.Name = name));

                EditorGUI.BeginChangeCheck();
                var include = EditorGUILayout.Toggle(UIText.Include, node.IncludeInExport);
                if (EditorGUI.EndChangeCheck()) c.Modify("Toggle Export", _ => ModifyNode(c, id, n => n.IncludeInExport = include));

                EditorGUI.BeginChangeCheck();
                var visible = EditorGUILayout.Toggle(UIText.Visible, node.Visible);
                if (EditorGUI.EndChangeCheck()) c.Modify("Toggle Visibility", _ => ModifyNode(c, id, n => n.Visible = visible));

                EditorGUI.BeginChangeCheck();
                var opacity = EditorGUILayout.Slider(UIText.Opacity, node.Opacity, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) c.Modify("Change Opacity", _ => ModifyNode(c, id, n => n.Opacity = opacity));

                if (node is MaskLayerNode layer)
                {
                    EditorGUI.BeginChangeCheck();
                    var mask = (Texture2D)EditorGUILayout.ObjectField(UIText.Mask, layer.Mask, typeof(Texture2D), false);
                    if (EditorGUI.EndChangeCheck()) c.Modify("Change Mask", _ => ModifyNode(c, id, n => ((MaskLayerNode)n).Mask = mask));

                    EditorGUI.BeginChangeCheck();
                    var channel = (MaskChannel)EditorGUILayout.Popup(UIText.MaskChannel, (int)layer.Channel, UIText.MaskChannelLabels);
                    if (EditorGUI.EndChangeCheck()) c.Modify("Change Mask Channel", _ => ModifyNode(c, id, n => ((MaskLayerNode)n).Channel = channel));

                    EditorGUI.BeginChangeCheck();
                    var invert = EditorGUILayout.Toggle(UIText.InvertMask, layer.InvertMask);
                    if (EditorGUI.EndChangeCheck()) c.Modify("Invert Mask", _ => ModifyNode(c, id, n => ((MaskLayerNode)n).InvertMask = invert));
                }
                else if (node is GroupNode group)
                {
                    EditorGUI.BeginChangeCheck();
                    var expanded = EditorGUILayout.Toggle(UIText.Expanded, group.Expanded);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ctx.Defer(() => c.Modify("Toggle Group", _ => SetExpanded(c, id, expanded)));
                    }
                }
            }
        }

        // Undo 後はノードのインスタンスが差し替わるため、変更時は必ず Id から引き直す
        private static void ModifyNode(PSDExporterController c, string id, System.Action<LayerTreeNode> mutation)
        {
            var node = c.FindNode(id);
            if (node != null) mutation(node);
        }

        private static void SetExpanded(PSDExporterController c, string id, bool expanded)
        {
            if (c.FindNode(id) is GroupNode group) group.Expanded = expanded;
        }
    }
}
