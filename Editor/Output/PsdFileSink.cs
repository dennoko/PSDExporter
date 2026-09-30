using System;
using System.IO;
using DennokoWorks.Tool.PSDExporter.Core.Psd;
using UnityEditor;

namespace DennokoWorks.Tool.PSDExporter.Output
{
    /// <summary>
    /// 一時ファイルへ書き込んでから置き換えることで、途中失敗時に壊れた PSD を残さない。
    /// 一時ファイル名は末尾 "~" で Unity のインポート対象外にする。
    /// </summary>
    public sealed class PsdFileSink : IPsdFileSink
    {
        private const int BufferSize = 1 << 20;

        public void Write(ResolvedOutput output, PsdDocument document)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (document == null) throw new ArgumentNullException(nameof(document));

            var path = output.FullPath;
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            var tempPath = path + ".tmp~";
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, BufferSize))
                {
                    PsdWriter.Write(stream, document);
                }

                if (File.Exists(path)) File.Replace(tempPath, path, null);
                else File.Move(tempPath, path);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }

            if (output.ImportAfterWrite && output.AssetPath != null)
            {
                AssetDatabase.ImportAsset(output.AssetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
