namespace Watson.Nodes.Services
{
    public interface INodeService
    {
        Task<Node> CreateAsync(NodeKind kind, string scope, string canonicalKey, IEnumerable<NodeAttribute> attributes, CancellationToken cancellationToken = default);

        Task<Node?> GetAsync(Guid nodeId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Node>> SearchAsync(NodeKind kind, string? query, string? scope, string? locale, int take, CancellationToken cancellationToken = default);

        Task<Node> PatchAttributesAsync(Guid nodeId, IEnumerable<NodeAttribute> set, IEnumerable<(string Key, string? Locale)> remove, CancellationToken cancellationToken = default);
    }
}
