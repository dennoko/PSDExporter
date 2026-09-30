using System;
using System.Collections.Generic;
using System.IO;
using DennokoWorks.Tool.PSDExporter.Core.Psd.Encoding;
using DennokoWorks.Tool.PSDExporter.Core.Psd.Sections;

namespace DennokoWorks.Tool.PSDExporter.Core.Psd
{
    /// <summary>
    /// <see cref="PsdDocument"/> を PSD (8bit RGB, RLE) としてストリームへ書き出すファサード。
    /// レイヤーの並び・チャンネル圧縮は呼び出し側 (Composition) で済ませておくこと。
    /// </summary>
    public static class PsdWriter
    {
        public static void Write(Stream stream, PsdDocument document, IReadOnlyList<IImageResource> imageResources = null)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (document == null) throw new ArgumentNullException(nameof(document));
            Validate(document);

            var writer = new BigEndianWriter(stream);
            HeaderSectionWriter.Write(writer, document);
            ColorModeDataSectionWriter.Write(writer);
            ImageResourcesSectionWriter.Write(writer, imageResources);
            LayerAndMaskSectionWriter.Write(writer, document);
            ImageDataSectionWriter.Write(writer, document);
            stream.Flush();
        }

        private static void Validate(PsdDocument document)
        {
            if (document.Width <= 0 || document.Width > PsdDocument.MaxDimension)
                throw new ArgumentOutOfRangeException(nameof(document), $"Width must be 1..{PsdDocument.MaxDimension}.");
            if (document.Height <= 0 || document.Height > PsdDocument.MaxDimension)
                throw new ArgumentOutOfRangeException(nameof(document), $"Height must be 1..{PsdDocument.MaxDimension}.");

            var merged = document.MergedImage;
            if (merged != null && !merged.SizeEquals(document.Width, document.Height))
                throw new ArgumentException("Merged image size does not match the document size.", nameof(document));

            foreach (var layer in document.Layers)
            {
                if (layer.Mask != null && !layer.Channels.Exists(c => c.Id == PsdChannelData.UserMask))
                    throw new ArgumentException($"Layer '{layer.Name}' has mask info but no user mask channel.", nameof(document));
            }
        }
    }
}
