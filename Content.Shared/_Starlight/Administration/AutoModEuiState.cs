using System.ComponentModel.DataAnnotations;
using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration
{
    [Serializable, NetSerializable]
    public sealed class AutoModEuiState : EuiStateBase
    {
        public List<AutoModRule> Rules = new();
        [Serializable, NetSerializable]
        public struct AutoModRule
        {
            public int Id { get; set; }
            public string? Regex { get; set; }
            public int Severity { get; set; }
            public string? Message { get; set; }
            public int Count { get; set; }
            public bool IsEnabled { get; set; }
            public bool CancelSpeech { get; set; }
        }
    }
}
