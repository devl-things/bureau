using System.Diagnostics.CodeAnalysis;

namespace Watson.Nodes
{
    public class NodeAttributeKey
    {
        public required string Key { get; init; }
        public string? Locale { get; init; } = null;

        [SetsRequiredMembers]
        public NodeAttributeKey(string key)
        {
            Key = key;
        }

        [SetsRequiredMembers]
        public NodeAttributeKey(string key, string? locale) : this(key)
        {
            Locale = locale;
        }
    }
}
