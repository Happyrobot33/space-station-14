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
    }

    public static class AutoModEuiMsg
    {
        [Serializable, NetSerializable]
        public sealed class DeleteRuleRequest : EuiMessageBase
        {
            public AutoModRule Rule { get; set; }
            public DeleteRuleRequest(AutoModRule rule)
            {
                Rule = rule;
            }
        }
    }
}
