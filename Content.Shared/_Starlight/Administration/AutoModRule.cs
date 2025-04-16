using Robust.Shared.Serialization;

namespace Content.Shared.Administration
{
    [Serializable, NetSerializable]
    public sealed class AutoModRule
    {
        public int Id { get; set; }
        public string? Regex { get; set; }
        public AutoModSeverity Severity { get; set; }
        public string? Message { get; set; }
        public int Count { get; set; }
        public bool IsEnabled { get; set; }
        public bool CancelSpeech { get; set; }
    }

    public enum AutoModSeverity
    {
        None = 0,
        Warning = 1,
        Kick = 2,
        Ban = 3,
    }
}
