using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Model;

namespace DennokoWorks.Tool.PSDExporter.App
{
    /// <summary>
    /// レイヤーツリーに対する純粋な操作。Undo やイベント通知は行わない (Controller の責務)。
    /// Children は「上 → 下」の順であることに注意。
    /// </summary>
    public static class LayerTreeOperations
    {
        public static LayerTreeNode FindById(GroupNode root, string id)
        {
            if (root == null || string.IsNullOrEmpty(id)) return null;
            if (root.Id == id) return root;

            foreach (var node in EnumerateDescendants(root))
            {
                if (node.Id == id) return node;
            }
            return null;
        }

        public static bool TryFindParent(GroupNode root, string id, out GroupNode parent, out int index)
        {
            parent = null;
            index = -1;
            if (root == null || string.IsNullOrEmpty(id)) return false;

            for (int i = 0; i < root.Children.Count; i++)
            {
                var child = root.Children[i];
                if (child == null) continue;
                if (child.Id == id)
                {
                    parent = root;
                    index = i;
                    return true;
                }
                if (child is GroupNode group && TryFindParent(group, id, out parent, out index)) return true;
            }
            return false;
        }

        /// <summary>深さ優先・上から順に全子孫を列挙する (root 自身は含まない)。</summary>
        public static IEnumerable<LayerTreeNode> EnumerateDescendants(GroupNode root)
        {
            if (root == null) yield break;

            foreach (var child in root.Children)
            {
                if (child == null) continue;
                yield return child;
                if (child is GroupNode group)
                {
                    foreach (var nested in EnumerateDescendants(group)) yield return nested;
                }
            }
        }

        /// <summary>
        /// マスクレイヤーを列挙する。<paramref name="exportedOnly"/> なら自身または祖先が
        /// エクスポート対象外のものを除く。
        /// </summary>
        public static IEnumerable<MaskLayerNode> EnumerateMaskLayers(GroupNode root, bool exportedOnly)
        {
            if (root == null) yield break;

            foreach (var child in root.Children)
            {
                if (child == null) continue;
                if (exportedOnly && !child.IncludeInExport) continue;

                if (child is MaskLayerNode layer)
                {
                    yield return layer;
                }
                else if (child is GroupNode group)
                {
                    foreach (var nested in EnumerateMaskLayers(group, exportedOnly)) yield return nested;
                }
            }
        }

        /// <summary>node が ancestor 自身またはその子孫なら true。</summary>
        public static bool IsSelfOrDescendant(LayerTreeNode ancestor, LayerTreeNode node)
        {
            if (ancestor == null || node == null) return false;
            if (ReferenceEquals(ancestor, node)) return true;
            if (!(ancestor is GroupNode group)) return false;

            foreach (var descendant in EnumerateDescendants(group))
            {
                if (ReferenceEquals(descendant, node)) return true;
            }
            return false;
        }

        public static void Insert(GroupNode parent, int index, LayerTreeNode node)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (node == null) throw new ArgumentNullException(nameof(node));

            index = Clamp(index, 0, parent.Children.Count);
            parent.Children.Insert(index, node);
        }

        public static bool Remove(GroupNode root, string id)
        {
            if (!TryFindParent(root, id, out var parent, out var index)) return false;
            parent.Children.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// ノードを newParent の index 位置 (移動前のインデックス基準) へ移動する。
        /// 自分自身や子孫の中への移動 (循環) は拒否する。
        /// </summary>
        public static bool Move(GroupNode root, string id, GroupNode newParent, int index)
        {
            if (newParent == null) return false;
            if (!TryFindParent(root, id, out var oldParent, out var oldIndex)) return false;

            var node = oldParent.Children[oldIndex];
            if (IsSelfOrDescendant(node, newParent)) return false;

            oldParent.Children.RemoveAt(oldIndex);
            if (ReferenceEquals(oldParent, newParent) && oldIndex < index) index--;

            Insert(newParent, index, node);
            return true;
        }

        /// <summary>同じ親の中で 1 つ上 (delta = -1) / 下 (delta = +1) に移動する。</summary>
        public static bool MoveWithinParent(GroupNode root, string id, int delta)
        {
            if (!TryFindParent(root, id, out var parent, out var index)) return false;

            int target = index + delta;
            if (target < 0 || target >= parent.Children.Count) return false;

            var node = parent.Children[index];
            parent.Children.RemoveAt(index);
            parent.Children.Insert(target, node);
            return true;
        }

        /// <summary>直上の兄弟がグループなら、その末尾 (最下段) へ入れる。</summary>
        public static bool Indent(GroupNode root, string id)
        {
            if (!TryFindParent(root, id, out var parent, out var index)) return false;
            if (index == 0) return false;
            if (!(parent.Children[index - 1] is GroupNode target)) return false;

            return Move(root, id, target, target.Children.Count);
        }

        /// <summary>親グループから出して、親のすぐ下に置く。</summary>
        public static bool Outdent(GroupNode root, string id)
        {
            if (!TryFindParent(root, id, out var parent, out _)) return false;
            if (ReferenceEquals(parent, root)) return false;
            if (!TryFindParent(root, parent.Id, out var grandParent, out var parentIndex)) return false;

            return Move(root, id, grandParent, parentIndex + 1);
        }

        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    }
}
