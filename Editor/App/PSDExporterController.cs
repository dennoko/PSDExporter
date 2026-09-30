using System;
using System.Collections.Generic;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.App.Validation;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.Output;
using DennokoWorks.Tool.PSDExporter.Pipeline;
using UnityEditor;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.App
{
    /// <summary>
    /// UI から呼ばれる唯一の窓口。UI 技術 (IMGUI / UI Toolkit) には依存しない。
    /// 全てのモデル変更は Undo 記録と変更通知を伴う。
    /// </summary>
    public sealed class PSDExporterController
    {
        private readonly PSDExporterState _state;
        private readonly ExportPipeline _pipeline;
        private readonly ProjectValidator _validator;

        public PSDExporterController(PSDExporterState state, ExportPipeline pipeline, ProjectValidator validator)
        {
            _state = state ? state : throw new ArgumentNullException(nameof(state));
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public static PSDExporterController CreateDefault(PSDExporterState state)
        {
            return new PSDExporterController(state, ExportPipeline.CreateDefault(), ProjectValidator.CreateDefault());
        }

        public ExportProject Project => _state.Project;
        public GroupNode Root => _state.Project.Root;

        /// <summary>変更ごとに加算される。キャッシュ無効化に使う。</summary>
        public int Revision => _state.Revision;

        public event Action Changed
        {
            add => _state.Changed += value;
            remove => _state.Changed -= value;
        }

        // ---------------------------------------------------------------- 汎用

        /// <summary>任意のモデル変更を Undo 付きで行う (プロパティ編集用)。</summary>
        public void Modify(string undoName, Action<ExportProject> mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));

            Undo.RecordObject(_state, undoName);
            mutation(_state.Project);
            _state.NotifyChanged();
        }

        /// <summary>Undo/Redo など外部要因でモデルが変わったときに呼ぶ。</summary>
        public void NotifyExternalChange() => _state.NotifyChanged();

        // ---------------------------------------------------------------- スロット

        public void AddSlot(Texture2D texture = null)
        {
            Modify("Add Texture Slot", p => p.Slots.Add(CreateSlot(p, texture)));
        }

        public void AddSlots(IEnumerable<Texture2D> textures)
        {
            var list = textures?.Where(t => t != null).ToList();
            if (list == null || list.Count == 0) return;

            Modify("Add Texture Slots", p =>
            {
                foreach (var texture in list) p.Slots.Add(CreateSlot(p, texture));
            });
        }

        public void RemoveSlot(int index)
        {
            if (index < 0 || index >= Project.Slots.Count) return;
            Modify("Remove Texture Slot", p => p.Slots.RemoveAt(index));
        }

        public void MoveSlot(int from, int to)
        {
            var slots = Project.Slots;
            if (from < 0 || from >= slots.Count || to < 0 || to >= slots.Count || from == to) return;

            Modify("Move Texture Slot", p =>
            {
                var slot = p.Slots[from];
                p.Slots.RemoveAt(from);
                p.Slots.Insert(to, slot);
            });
        }

        private static TextureSlot CreateSlot(ExportProject project, Texture2D texture)
        {
            return new TextureSlot
            {
                Texture = texture,
                Suffix = SlotSuffixSuggester.Suggest(texture, project.Slots.Select(s => s.Suffix)),
            };
        }

        // ---------------------------------------------------------------- レイヤーツリー

        public LayerTreeNode FindNode(string id) => LayerTreeOperations.FindById(Root, id);

        /// <param name="parentId">追加先グループの Id。null ならルート。</param>
        /// <param name="index">追加位置 (0 = 最上段)。</param>
        /// <param name="mask">マスクテクスチャ。</param>
        public MaskLayerNode AddLayer(string parentId, int index, Texture2D mask = null)
        {
            var parent = ResolveGroup(parentId);
            if (parent == null) return null;

            var layer = CreateLayer(mask);
            Modify("Add Layer", _ => LayerTreeOperations.Insert(parent, index, layer));
            return layer;
        }

        /// <summary>複数のマスクから一括でレイヤーを追加する (名前 = テクスチャ名)。</summary>
        public void AddLayers(string parentId, int index, IEnumerable<Texture2D> masks)
        {
            var parent = ResolveGroup(parentId);
            var list = masks?.Where(t => t != null).ToList();
            if (parent == null || list == null || list.Count == 0) return;

            Modify("Add Layers", _ =>
            {
                for (int i = 0; i < list.Count; i++) LayerTreeOperations.Insert(parent, index + i, CreateLayer(list[i]));
            });
        }

        public GroupNode AddGroup(string parentId, int index)
        {
            var parent = ResolveGroup(parentId);
            if (parent == null) return null;

            var group = new GroupNode { Name = NextName("Group") };
            Modify("Add Group", _ => LayerTreeOperations.Insert(parent, index, group));
            return group;
        }

        public void RemoveNode(string nodeId)
        {
            if (FindNode(nodeId) == null || nodeId == Root.Id) return;
            Modify("Remove Layer", _ => LayerTreeOperations.Remove(Root, nodeId));
        }

        /// <summary>循環 (自分の子孫への移動) は拒否し false を返す。</summary>
        public bool MoveNode(string nodeId, string newParentId, int index)
        {
            var node = FindNode(nodeId);
            var parent = ResolveGroup(newParentId);
            if (node == null || parent == null || LayerTreeOperations.IsSelfOrDescendant(node, parent)) return false;

            bool moved = false;
            Modify("Move Layer", _ => moved = LayerTreeOperations.Move(Root, nodeId, parent, index));
            return moved;
        }

        public bool MoveUp(string nodeId) => TryTreeEdit("Move Layer Up", () => LayerTreeOperations.MoveWithinParent(Root, nodeId, -1), CanMoveWithinParent(nodeId, -1));
        public bool MoveDown(string nodeId) => TryTreeEdit("Move Layer Down", () => LayerTreeOperations.MoveWithinParent(Root, nodeId, +1), CanMoveWithinParent(nodeId, +1));
        public bool Indent(string nodeId) => TryTreeEdit("Indent Layer", () => LayerTreeOperations.Indent(Root, nodeId), CanIndent(nodeId));
        public bool Outdent(string nodeId) => TryTreeEdit("Outdent Layer", () => LayerTreeOperations.Outdent(Root, nodeId), CanOutdent(nodeId));

        public bool CanMoveWithinParent(string nodeId, int delta)
        {
            if (!LayerTreeOperations.TryFindParent(Root, nodeId, out var parent, out var index)) return false;
            int target = index + delta;
            return target >= 0 && target < parent.Children.Count;
        }

        public bool CanIndent(string nodeId)
        {
            return LayerTreeOperations.TryFindParent(Root, nodeId, out var parent, out var index)
                   && index > 0
                   && parent.Children[index - 1] is GroupNode;
        }

        public bool CanOutdent(string nodeId)
        {
            return LayerTreeOperations.TryFindParent(Root, nodeId, out var parent, out _)
                   && !ReferenceEquals(parent, Root);
        }

        /// <summary>同じ親を持つノード群を新しいグループにまとめる。</summary>
        public GroupNode GroupNodes(IReadOnlyList<string> nodeIds)
        {
            if (nodeIds == null || nodeIds.Count == 0) return null;

            GroupNode parent = null;
            var indices = new List<int>();
            foreach (var id in nodeIds)
            {
                if (!LayerTreeOperations.TryFindParent(Root, id, out var p, out var index)) return null;
                if (parent != null && !ReferenceEquals(parent, p)) return null;
                parent = p;
                indices.Add(index);
            }

            var ordered = indices.Distinct().OrderBy(i => i).ToList();
            var group = new GroupNode { Name = NextName("Group") };

            Modify("Group Layers", _ =>
            {
                var nodes = ordered.Select(i => parent.Children[i]).ToList();
                for (int i = ordered.Count - 1; i >= 0; i--) parent.Children.RemoveAt(ordered[i]);
                group.Children.AddRange(nodes);
                LayerTreeOperations.Insert(parent, ordered[0], group);
            });
            return group;
        }

        private bool TryTreeEdit(string undoName, Func<bool> edit, bool canEdit)
        {
            if (!canEdit) return false;

            bool changed = false;
            Modify(undoName, _ => changed = edit());
            return changed;
        }

        private GroupNode ResolveGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) return Root;
            return FindNode(groupId) as GroupNode;
        }

        private MaskLayerNode CreateLayer(Texture2D mask)
        {
            return new MaskLayerNode
            {
                Name = mask != null ? mask.name : NextName("Layer"),
                Mask = mask,
            };
        }

        private string NextName(string baseName)
        {
            var names = new HashSet<string>(LayerTreeOperations.EnumerateDescendants(Root).Select(n => n.Name));
            for (int i = 1; ; i++)
            {
                var name = $"{baseName} {i}";
                if (!names.Contains(name)) return name;
            }
        }

        // ---------------------------------------------------------------- 検証・実行

        public List<ValidationMessage> Validate() => _validator.Validate(Project);

        /// <summary>出力予定のファイル (上書き確認用)。</summary>
        public List<ResolvedOutput> PlanOutputs() => OutputPathResolver.Resolve(Project);

        /// <param name="progress">進捗通知・キャンセル。</param>
        /// <param name="overwriteConfirmed">既存ファイルの上書きがユーザーに承認済みか。</param>
        public ExportReport Export(IProgressReporter progress, bool overwriteConfirmed)
        {
            var messages = Validate();
            if (ProjectValidator.HasErrors(messages))
            {
                var report = new ExportReport();
                report.Errors.AddRange(messages.Where(m => m.Severity == ValidationSeverity.Error).Select(m => m.Text));
                return report;
            }

            return _pipeline.Run(Project, progress, overwriteConfirmed);
        }
    }
}
