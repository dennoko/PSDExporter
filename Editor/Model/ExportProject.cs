using System;
using System.Collections.Generic;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Model
{
    /// <summary>エクスポート設定一式のルート。純粋なデータでロジックは持たない。</summary>
    [Serializable]
    public sealed class ExportProject
    {
        public List<TextureSlot> Slots = new List<TextureSlot>();

        /// <summary>仮想ルート。ルート自身は PSD に出力されない。</summary>
        [SerializeReference]
        public GroupNode Root = new GroupNode { Name = "<root>" };

        public ExportOptions Options = new ExportOptions();
        public OutputSettings Output = new OutputSettings();

        /// <summary>有効かつテクスチャが設定されたスロットを列挙する。</summary>
        public IEnumerable<SlotEntry> EnumerateExportableSlots()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                var slot = Slots[i];
                if (slot != null && slot.Enabled && slot.Texture != null) yield return new SlotEntry(i, slot);
            }
        }
    }

    public readonly struct SlotEntry
    {
        public readonly int Index;
        public readonly TextureSlot Slot;

        public SlotEntry(int index, TextureSlot slot)
        {
            Index = index;
            Slot = slot;
        }
    }
}
