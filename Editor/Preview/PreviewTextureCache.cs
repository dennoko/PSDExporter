using System;
using System.Collections.Generic;
using DennokoWorks.Tool.PSDExporter.Core.Imaging;
using DennokoWorks.Tool.PSDExporter.TextureIO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DennokoWorks.Tool.PSDExporter.Preview
{
    /// <summary>プレビュー用に生成した Texture2D を所有し、置き換え・破棄時に DestroyImmediate する。</summary>
    public sealed class PreviewTextureCache : IDisposable
    {
        private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        public Texture2D Get(string key)
        {
            return key != null && _textures.TryGetValue(key, out var texture) ? texture : null;
        }

        public void Set(string key, RgbaImage image)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            Remove(key);
            if (image != null) _textures[key] = UnityImageConverter.ToTexture2D(image);
        }

        public void Remove(string key)
        {
            if (_textures.TryGetValue(key, out var texture))
            {
                if (texture != null) Object.DestroyImmediate(texture);
                _textures.Remove(key);
            }
        }

        public void Clear()
        {
            foreach (var texture in _textures.Values)
            {
                if (texture != null) Object.DestroyImmediate(texture);
            }
            _textures.Clear();
        }

        public void Dispose() => Clear();
    }
}
