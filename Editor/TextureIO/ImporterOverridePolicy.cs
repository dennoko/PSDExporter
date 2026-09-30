using DennokoWorks.Tool.PSDExporter.Model;
using UnityEditor;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>
    /// ピクセル取得のためにインポート設定をどう一時変更するか。
    /// <see cref="SourceReadQuality.Original"/> では圧縮・サイズ制限・型変換を解除して元ファイル品質で読む。
    /// </summary>
    public sealed class ImporterOverridePolicy
    {
        public const int MaxTextureSize = 16384;

        public SourceReadQuality Quality { get; }

        public ImporterOverridePolicy(SourceReadQuality quality)
        {
            Quality = quality;
        }

        public bool RequiresChange(ImporterSettingsSnapshot current)
        {
            if (!current.IsReadable) return true;
            if (Quality == SourceReadQuality.AsImported) return false;

            return current.TextureCompression != TextureImporterCompression.Uncompressed
                   || current.CrunchedCompression
                   || current.MaxTextureSize < MaxTextureSize
                   || current.NpotScale != TextureImporterNPOTScale.None
                   || RequiresTypeChange(current.TextureType)
                   || current.PlatformOverridden;
        }

        public void Apply(TextureImporter importer, string platformName)
        {
            importer.isReadable = true;
            if (Quality == SourceReadQuality.AsImported) return;

            // NormalMap / SingleChannel 等はスウィズルやチャンネル変換がかかるため Default として読む
            if (RequiresTypeChange(importer.textureType))
            {
                importer.textureType = TextureImporterType.Default;
                importer.convertToNormalmap = false;
            }

            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = MaxTextureSize;
            importer.npotScale = TextureImporterNPOTScale.None;

            if (!string.IsNullOrEmpty(platformName))
            {
                var platform = importer.GetPlatformTextureSettings(platformName);
                if (platform.overridden)
                {
                    platform.overridden = false;
                    importer.SetPlatformTextureSettings(platform);
                }
            }
        }

        private static bool RequiresTypeChange(TextureImporterType type)
        {
            switch (type)
            {
                case TextureImporterType.Default:
                case TextureImporterType.Sprite:
                case TextureImporterType.GUI:
                    return false;
                default:
                    return true;
            }
        }
    }
}
