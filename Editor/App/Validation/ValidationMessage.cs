namespace DennokoWorks.Tool.PSDExporter.App.Validation
{
    public enum ValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
    }

    public sealed class ValidationMessage
    {
        public ValidationSeverity Severity { get; }
        public string Text { get; }

        /// <summary>関連するレイヤーノードの Id。無ければ null。</summary>
        public string NodeId { get; }

        /// <summary>関連するスロットのインデックス。無ければ -1。</summary>
        public int SlotIndex { get; }

        public ValidationMessage(ValidationSeverity severity, string text, string nodeId = null, int slotIndex = -1)
        {
            Severity = severity;
            Text = text;
            NodeId = nodeId;
            SlotIndex = slotIndex;
        }

        public static ValidationMessage Error(string text, string nodeId = null, int slotIndex = -1)
            => new ValidationMessage(ValidationSeverity.Error, text, nodeId, slotIndex);

        public static ValidationMessage Warning(string text, string nodeId = null, int slotIndex = -1)
            => new ValidationMessage(ValidationSeverity.Warning, text, nodeId, slotIndex);

        public static ValidationMessage Info(string text, string nodeId = null, int slotIndex = -1)
            => new ValidationMessage(ValidationSeverity.Info, text, nodeId, slotIndex);

        public override string ToString() => $"[{Severity}] {Text}";
    }
}
