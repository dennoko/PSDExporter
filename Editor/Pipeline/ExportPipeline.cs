using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DennokoWorks.Tool.PSDExporter.Core.Composition;
using DennokoWorks.Tool.PSDExporter.Model;
using DennokoWorks.Tool.PSDExporter.Output;
using DennokoWorks.Tool.PSDExporter.TextureIO;
using UnityEngine;

namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    /// <summary>
    /// エクスポート全体のオーケストレーション。依存は全てコンストラクタで注入する。
    /// 入力の検証は呼び出し側 (App 層) の責務とし、ここでは実行のみを行う。
    /// </summary>
    public sealed class ExportPipeline
    {
        /// <summary>テクスチャ群と読み込み品質から、読み込み準備済みのソースを開く。</summary>
        public delegate ITextureSource TextureSourceFactory(IEnumerable<Texture2D> textures, SourceReadQuality quality);

        // 全体進捗のうちインポート設定変更に割り当てる割合
        private const float PrepareProgress = 0.1f;

        private readonly TextureSourceFactory _sourceFactory;
        private readonly IPsdFileSink _sink;
        private readonly Func<CutoutMode, LayerComposer> _composerFactory;

        public ExportPipeline(TextureSourceFactory sourceFactory, IPsdFileSink sink, Func<CutoutMode, LayerComposer> composerFactory)
        {
            _sourceFactory = sourceFactory ?? throw new ArgumentNullException(nameof(sourceFactory));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _composerFactory = composerFactory ?? throw new ArgumentNullException(nameof(composerFactory));
        }

        public static ExportPipeline CreateDefault()
        {
            return new ExportPipeline(
                (textures, quality) => TextureAccessSession.Open(textures, quality),
                new PsdFileSink(),
                LayerComposer.CreateDefault);
        }

        /// <param name="project">エクスポート設定。</param>
        /// <param name="progress">進捗通知・キャンセル。</param>
        /// <param name="overwriteConfirmed">OverwritePolicy.Ask で既存ファイルの上書きがユーザーに承認済みか。</param>
        public ExportReport Run(ExportProject project, IProgressReporter progress, bool overwriteConfirmed)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            progress = progress ?? NullProgressReporter.Instance;

            var report = new ExportReport();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                RunCore(project, progress, overwriteConfirmed, report);
            }
            catch (OperationCanceledException)
            {
                report.Cancelled = true;
            }
            catch (Exception e)
            {
                report.Errors.Add(e is ExportException ? e.Message : $"{e.GetType().Name}: {e.Message}");
                if (!(e is ExportException)) UnityEngine.Debug.LogException(e);
            }

            report.Elapsed = stopwatch.Elapsed;
            return report;
        }

        private void RunCore(ExportProject project, IProgressReporter progress, bool overwriteConfirmed, ExportReport report)
        {
            var slots = project.EnumerateExportableSlots().ToList();
            if (slots.Count == 0) throw new ExportException("エクスポートするテクスチャスロットがありません。");

            var outputs = OutputPathResolver.Resolve(project);
            if (project.Output.Overwrite == OverwritePolicy.Ask && !overwriteConfirmed && outputs.Any(o => o.Exists))
            {
                throw new ExportException("既存ファイルの上書きが確認されていません。");
            }

            progress.Report(0f, "テクスチャのインポート設定を準備中...");
            var textures = ProjectResolver.CollectTextures(project, slots);

            using (var source = _sourceFactory(textures, project.Options.ReadQuality))
            using (var masks = new MaskCache(source))
            {
                var composer = _composerFactory(project.Options.CutoutMode);
                float span = (1f - PrepareProgress) / outputs.Count;

                for (int i = 0; i < outputs.Count; i++)
                {
                    if (progress.IsCancelled) throw new OperationCanceledException();

                    var output = outputs[i];
                    var slot = project.Slots[output.SlotIndex];
                    var label = string.IsNullOrWhiteSpace(slot.Suffix) ? slot.Texture.name : slot.Suffix;
                    float start = PrepareProgress + span * i;

                    try
                    {
                        progress.Report(start, $"{label}: 読み込み中...");
                        var pixels = source.Read(slot.Texture);
                        var request = ProjectResolver.Resolve(project, pixels, masks);

                        var observer = new ProgressCompositionObserver(progress, start, span * 0.9f, label, report.Warnings);
                        var document = composer.Compose(request, observer);

                        progress.Report(start + span * 0.9f, $"{label}: 書き込み中...");
                        _sink.Write(output, document);

                        report.AddSuccess(output.SlotIndex, label, output.FullPath);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        // 1 スロットの失敗で他のスロットを止めない
                        report.AddFailure(output.SlotIndex, label, e.Message, e);
                    }
                }

                report.Warnings.AddRange(source.Warnings);
            }
        }
    }
}
