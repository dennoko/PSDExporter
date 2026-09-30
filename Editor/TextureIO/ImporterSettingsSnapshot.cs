using System;
using UnityEditor;

namespace DennokoWorks.Tool.PSDExporter.TextureIO
{
    /// <summary>
    /// 一時変更する TextureImporter 設定の変更前の値。JSON 化して復元ジャーナルにも保存する。
    /// 変更対象の項目だけを保持・復元し、それ以外の設定には触れない。
    /// </summary>
    [Serializable]
    public sealed class ImporterSettingsSnapshot
    {
        public string AssetPath;

        public bool IsReadable;
        public TextureImporterCompression TextureCompression;
        public bool CrunchedCompression;
        public int MaxTextureSize;
        public TextureImporterNPOTScale NpotScale;
        public TextureImporterType TextureType;
        public bool SRGBTexture;
        public bool ConvertToNormalmap;

        /// <summary>アクティブビルドターゲットのプラットフォーム名 ("Standalone" 等)。</summary>
        public string PlatformName;
        public bool PlatformOverridden;

        public static ImporterSettingsSnapshot Capture(string assetPath, TextureImporter importer, string platformName)
        {
            if (importer == null) throw new ArgumentNullException(nameof(importer));

            var snapshot = new ImporterSettingsSnapshot
            {
                AssetPath = assetPath,
                IsReadable = importer.isReadable,
                TextureCompression = importer.textureCompression,
                CrunchedCompression = importer.crunchedCompression,
                MaxTextureSize = importer.maxTextureSize,
                NpotScale = importer.npotScale,
                TextureType = importer.textureType,
                SRGBTexture = importer.sRGBTexture,
                ConvertToNormalmap = importer.convertToNormalmap,
                PlatformName = platformName,
            };

            if (!string.IsNullOrEmpty(platformName))
            {
                snapshot.PlatformOverridden = importer.GetPlatformTextureSettings(platformName).overridden;
            }

            return snapshot;
        }

        public void Restore(TextureImporter importer)
        {
            if (importer == null) throw new ArgumentNullException(nameof(importer));

            // textureType を先に戻す (型変更で他の値が補正される可能性があるため、個別値は後から上書きする)
            importer.textureType = TextureType;
            importer.isReadable = IsReadable;
            importer.textureCompression = TextureCompression;
            importer.crunchedCompression = CrunchedCompression;
            importer.maxTextureSize = MaxTextureSize;
            importer.npotScale = NpotScale;
            importer.sRGBTexture = SRGBTexture;
            importer.convertToNormalmap = ConvertToNormalmap;

            if (!string.IsNullOrEmpty(PlatformName))
            {
                var platform = importer.GetPlatformTextureSettings(PlatformName);
                if (platform.overridden != PlatformOverridden)
                {
                    platform.overridden = PlatformOverridden;
                    importer.SetPlatformTextureSettings(platform);
                }
            }
        }
    }
}
