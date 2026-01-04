namespace Watson.Nodes.Contracts.Dtos
{
    public sealed class ChangesResponse
    {
        public required IReadOnlyList<ChangeEventDto> Events { get; init; }
        public required long NextCursor { get; init; }
        public required ChangeModeContract Mode { get; init; }
    }
}
