using System.Diagnostics.CodeAnalysis;
using Watson.Nodes.Abstractions.Conventions;

namespace Watson.Nodes
{
    public class NodeAttributeKey
    {
        public required string Key { get; init; }
        public string Locale { get; init; } = LocaleConventions.DefaultLocale;

        [SetsRequiredMembers]
        public NodeAttributeKey(string key)
        {
            Key = key;
        }

        [SetsRequiredMembers]
        public NodeAttributeKey(string key, string? locale) : this(key)
        {
            Locale = locale ?? LocaleConventions.DefaultLocale;
        }
    }
}
