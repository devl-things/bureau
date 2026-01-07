using System.Diagnostics.CodeAnalysis;

namespace Watson.Nodes
{
    public class NodeAttributeKey
    {
        public required string Key { get; init; }
        public string? Locale { get; init; }

        [SetsRequiredMembers]
        public NodeAttributeKey(string key)
        {
            Key = key;
            Locale = null;
        }
    }
}
