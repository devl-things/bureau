namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class ChangeEventDto
    {
        public required long Sequence { get; init; }
        public required string Type { get; init; } // NodeCreated, NodeUpdated, NodeArchived
        public required NodeKindContract Kind { get; init; }
        public required Guid NodeId { get; init; }
        public required int Version { get; init; }
        public required ChangePayloadDto Payload { get; init; }
    }
}
