using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.App
{
    /// <summary>テクスチャ名から出力サフィックスの候補を推測する。</summary>
    public static class SlotSuffixSuggester
    {
        /// <summary>UI で選択肢として提示する代表的なサフィックス。</summary>
        public static readonly string[] CommonSuffixes =
        {
            "BaseColor", "Normal", "MetallicSmoothness", "Emission", "Occlusion", "Mask",
        };

        private static readonly (string keyword, string suffix)[] Keywords =
        {
            ("normal", "Normal"),
            ("nrm", "Normal"),
            ("metal", "MetallicSmoothness"),
            ("smooth", "MetallicSmoothness"),
            ("rough", "Roughness"),
            ("emis", "Emission"),
            ("emit", "Emission"),
            ("occlusion", "Occlusion"),
            ("_ao", "Occlusion"),
            ("albedo", "BaseColor"),
            ("basecolor", "BaseColor"),
            ("base_color", "BaseColor"),
            ("diffuse", "BaseColor"),
            ("maintex", "BaseColor"),
        };

        public static string Suggest(Texture2D texture, IEnumerable<string> usedSuffixes)
        {
            var used = new HashSet<string>(usedSuffixes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            string candidate = null;
            if (texture != null)
            {
                var name = texture.name.ToLowerInvariant();
                foreach (var (keyword, suffix) in Keywords)
                {
                    if (name.Contains(keyword))
                    {
                        candidate = suffix;
                        break;
                    }
                }
                if (candidate == null) candidate = texture.name;
            }

            if (candidate == null)
            {
                candidate = CommonSuffixes.FirstOrDefault(s => !used.Contains(s)) ?? "Texture";
            }

            if (!used.Contains(candidate)) return candidate;

            for (int i = 2; ; i++)
            {
                var numbered = $"{candidate}{i}";
                if (!used.Contains(numbered)) return numbered;
            }
        }
    }
}
