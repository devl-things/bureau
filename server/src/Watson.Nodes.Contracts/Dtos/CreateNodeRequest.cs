namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class CreateNodeRequest
    {
        public string? CanonicalKey { get; init; }
        public string? Scope { get; init; } // default "global"
        public IReadOnlyList<AttributeDto>? Attributes { get; init; }
    }
}
