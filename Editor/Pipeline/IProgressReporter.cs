namespace DennokoWorks.Tool.PSDExporter.Pipeline
{
    public interface IProgressReporter
    {
        /// <param name="progress">0..1</param>
        /// <param name="message">表示メッセージ。</param>
        void Report(float progress, string message);

        bool IsCancelled { get; }
    }

    public sealed class NullProgressReporter : IProgressReporter
    {
        public static readonly NullProgressReporter Instance = new NullProgressReporter();

        public void Report(float progress, string message)
        {
        }

        public bool IsCancelled => false;
    }
}
