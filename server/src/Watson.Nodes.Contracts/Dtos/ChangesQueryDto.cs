namespace Watson.Nodes.Contracts.Dtos
{
    /// <summary>
    /// Query parameters for the change feed endpoint.
    /// </summary>
    public sealed class ChangesQueryDto : CursorQueryDto
    {
        /// <summary>
        /// Controls the level of detail returned by the change feed.
        /// </summary>
        /// <remarks>
        /// <c>Compact</c> returns at most the latest event per NodeId within the polling window.
        /// <c>Full</c> returns all events within the window.
        /// </remarks>
        public ChangeModeContract Mode { get; set; } = ChangeModeContract.Compact;
    }
}
