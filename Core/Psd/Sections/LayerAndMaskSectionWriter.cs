using System;
using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd.Sections
{
    /// <summary>Layer and Mask Information セクション。</summary>
    internal static class LayerAndMaskSectionWriter
    {
        private const byte FlagHidden = 0x02;
        private const byte FlagBit4Meaningful = 0x08;
        private const byte FlagPixelDataIrrelevant = 0x10;

        private const byte MaskFlagDisabled = 0x02;

        public static void Write(BigEndianWriter writer, PsdDocument document)
        {
            using (writer.BeginLength())
            {
                WriteLayerInfo(writer, document);

                // Global Layer Mask Info: なし
                writer.WriteUInt32(0);
            }
        }

        private static void WriteLayerInfo(BigEndianWriter writer, PsdDocument document)
        {
            var layers = document.Layers;

            using (writer.BeginLength(2))
            {
                if (layers.Count == 0) return;
                if (layers.Count > short.MaxValue) throw new InvalidOperationException("Too many layers.");

                // 負数: マージ画像の先頭アルファチャンネルがマージ結果の透明度であることを示す
                writer.WriteInt16((short)-layers.Count);

                foreach (var layer in layers) WriteLayerRecord(writer, layer);

                foreach (var layer in layers)
                {
                    foreach (var channel in layer.Channels)
                    {
                        writer.WriteUInt16(channel.Compression);
                        writer.WriteBytes(channel.EncodedBytes);
                    }
                }
            }
        }

        private static void WriteLayerRecord(BigEndianWriter writer, PsdLayerRecord layer)
        {
            WriteRect(writer, layer.Bounds);

            writer.WriteUInt16((ushort)layer.Channels.Count);
            foreach (var channel in layer.Channels)
            {
                writer.WriteInt16(channel.Id);
                writer.WriteUInt32((uint)channel.Length);
            }

            writer.WriteSignature("8BIM");
            writer.WriteSignature(layer.BlendMode.ToKey());
            writer.WriteByte(layer.Opacity);
            writer.WriteByte(0); // clipping: base
            writer.WriteByte(BuildFlags(layer));
            writer.WriteByte(0); // filler

            using (writer.BeginLength())
            {
                WriteLayerMaskData(writer, layer.Mask);

                // Layer Blending Ranges: なし
                writer.WriteUInt32(0);

                PsdStringEncoder.WritePascal(writer, layer.Name, 4);

                AdditionalLayerInfoWriter.WriteUnicodeName(writer, layer.Name);
                if (layer.IsSectionRecord)
                {
                    AdditionalLayerInfoWriter.WriteSectionDivider(writer, layer.Section, layer.BlendMode);
                }
            }
        }

        private static byte BuildFlags(PsdLayerRecord layer)
        {
            byte flags = FlagBit4Meaningful;
            if (!layer.Visible) flags |= FlagHidden;
            if (layer.IsSectionRecord) flags |= FlagPixelDataIrrelevant;
            return flags;
        }

        private static void WriteLayerMaskData(BigEndianWriter writer, PsdLayerMask mask)
        {
            if (mask == null)
            {
                writer.WriteUInt32(0);
                return;
            }

            writer.WriteUInt32(20);
            WriteRect(writer, mask.Bounds);
            writer.WriteByte(mask.DefaultColor);
            writer.WriteByte(mask.Disabled ? MaskFlagDisabled : (byte)0);
            writer.WriteUInt16(0); // padding
        }

        private static void WriteRect(BigEndianWriter writer, PsdRect rect)
        {
            writer.WriteInt32(rect.Top);
            writer.WriteInt32(rect.Left);
            writer.WriteInt32(rect.Bottom);
            writer.WriteInt32(rect.Right);
        }
    }
}
