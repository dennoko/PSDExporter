using System.Collections.Generic;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.Model;

namespace DennokoWorks.Tool.PSDExporter.App.Validation.Rules
{
    /// <summary>
    /// 解像度・アスペクト比の不一致を知らせる。マスクは自動リサイズされるためエラーにはしない。
    /// (インポート設定の最大サイズ制限前の元解像度は読み込むまで分からないため、現在のテクスチャサイズで判定する)
    /// </summary>
    public sealed class ResolutionMismatchRule : IProjectValidationRule
    {
        private const float AspectTolerance = 0.01f;

        public IEnumerable<ValidationMessage> Validate(ExportProject project)
        {
            var slots = project.EnumerateExportableSlots().ToList();
            if (slots.Count == 0) yield break;

            var sizes = slots.Select(s => (s.Slot.Texture.width, s.Slot.Texture.height)).Distinct().ToList();
            if (sizes.Count > 1)
            {
                yield return ValidationMessage.Info("スロット間でテクスチャの解像度が異なります。マスクはスロットごとにリサイズされます。");
            }

            float slotAspect = Aspect(slots[0].Slot.Texture.width, slots[0].Slot.Texture.height);

            foreach (var layer in LayerTreeOperations.EnumerateMaskLayers(project.Root, exportedOnly: true))
            {
                if (layer.Mask == null) continue;

                float maskAspect = Aspect(layer.Mask.width, layer.Mask.height);
                if (System.Math.Abs(maskAspect - slotAspect) > AspectTolerance * slotAspect)
                {
                    yield return ValidationMessage.Warning(
                        $"レイヤー「{layer.Name}」のマスクはテクスチャとアスペクト比が異なります。引き伸ばして適用されます。", layer.Id);
                }
            }
        }

        private static float Aspect(int width, int height) => height == 0 ? 0f : (float)width / height;
    }
}
