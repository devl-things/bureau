namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class SearchNodesResponse
    {
        public required IReadOnlyList<NodeSummaryDto> Items { get; init; }
        public string? Next { get; init; }
    }
}
