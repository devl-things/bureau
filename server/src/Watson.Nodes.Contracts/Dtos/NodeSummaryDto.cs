namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class NodeSummaryDto
    {
        public required Guid NodeId { get; init; }
        public required NodeKindContract Kind { get; init; }
        public required string Scope { get; init; }
        public required string CanonicalKey { get; init; }
        public required NodeStatusContract Status { get; init; }

        // Convenience for UIs: best label match if present in attributes
        public string? Label { get; init; }
        public string? LabelLocale { get; init; }
    }
}
