namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class PatchNodeAttributesRequest
    {
        public IReadOnlyList<AttributeDto>? Set { get; init; }
        public IReadOnlyList<AttributeKeyDto>? Remove { get; init; }
    }
}
