using System;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.Core.Psd;
using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;

namespace DennokoWorks.Tool.PSDExporter.Core.Composition
{
    /// <summary>合成結果を PSD のレイヤーレコード (圧縮済みチャンネル) に変換する。</summary>
    public static class PsdLayerRecordFactory
    {
        public static PsdLayerRecord CreatePixelLayer(string name, ComposedLayer layer, bool visible, float opacity, bool trimBounds)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            var pixels = layer.Pixels;
            var canvas = PsdRect.FromSize(pixels.Width, pixels.Height);
            var bounds = trimBounds && layer.UserMask == null ? ComputeOpaqueBounds(pixels) : canvas;

            var record = new PsdLayerRecord
            {
                Name = name,
                Bounds = bounds,
                Opacity = ToByte(opacity),
                Visible = visible,
                BlendMode = PsdBlendMode.Normal,
            };

            record.Channels.Add(ChannelEncoder.EncodeLayerChannel(pixels, PsdChannelData.Transparency, bounds));
            record.Channels.Add(ChannelEncoder.EncodeLayerChannel(pixels, PsdChannelData.Red, bounds));
            record.Channels.Add(ChannelEncoder.EncodeLayerChannel(pixels, PsdChannelData.Green, bounds));
            record.Channels.Add(ChannelEncoder.EncodeLayerChannel(pixels, PsdChannelData.Blue, bounds));

            if (layer.UserMask != null)
            {
                record.Mask = new PsdLayerMask(canvas, defaultColor: 0);
                record.Channels.Add(ChannelEncoder.EncodeMaskChannel(layer.UserMask, canvas));
            }

            return record;
        }

        /// <summary>グループ本体 (フォルダ) のレコード。</summary>
        public static PsdLayerRecord CreateGroupBegin(GroupSpec group)
        {
            var record = CreateSectionRecord(group.Name,
                group.Expanded ? PsdSectionType.OpenFolder : PsdSectionType.ClosedFolder);
            record.Visible = group.Visible;
            record.Opacity = ToByte(group.Opacity);
            record.BlendMode = PsdBlendMode.PassThrough;
            return record;
        }

        /// <summary>グループ終端 ("&lt;/Layer group&gt;") のレコード。</summary>
        public static PsdLayerRecord CreateGroupEnd()
        {
            return CreateSectionRecord(PsdLayerRecord.GroupEndName, PsdSectionType.BoundingDivider);
        }

        private static PsdLayerRecord CreateSectionRecord(string name, PsdSectionType type)
        {
            var record = new PsdLayerRecord
            {
                Name = name,
                Bounds = PsdRect.Empty,
                Section = type,
                BlendMode = PsdBlendMode.Normal,
            };

            record.Channels.Add(PsdChannelData.Empty(PsdChannelData.Transparency));
            record.Channels.Add(PsdChannelData.Empty(PsdChannelData.Red));
            record.Channels.Add(PsdChannelData.Empty(PsdChannelData.Green));
            record.Channels.Add(PsdChannelData.Empty(PsdChannelData.Blue));
            return record;
        }

        private static byte ToByte(float opacity)
        {
            if (opacity <= 0f) return 0;
            if (opacity >= 1f) return 255;
            return (byte)Math.Round(opacity * 255f);
        }

        /// <summary>アルファが 0 でない画素を含む最小矩形。全透明なら空矩形。</summary>
        private static PsdRect ComputeOpaqueBounds(RgbaImage image)
        {
            int w = image.Width;
            int h = image.Height;
            var px = image.Pixels;

            int top = -1, bottom = -1, left = w, right = -1;

            for (int y = 0; y < h; y++)
            {
                int rowStart = y * w * 4 + 3;
                int first = -1, last = -1;
                for (int x = 0, p = rowStart; x < w; x++, p += 4)
                {
                    if (px[p] == 0) continue;
                    if (first < 0) first = x;
                    last = x;
                }

                if (first < 0) continue;
                if (top < 0) top = y;
                bottom = y;
                if (first < left) left = first;
                if (last > right) right = last;
            }

            return top < 0 ? PsdRect.Empty : new PsdRect(top, left, bottom + 1, right + 1);
        }
    }
}
