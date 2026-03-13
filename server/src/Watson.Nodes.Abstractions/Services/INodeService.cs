using Bureau;

namespace Watson.Nodes.Services
{
    /// <summary>
    /// Application service for creating, reading, updating, and searching nodes.
    /// </summary>
    /// <remarks>
    /// The service is the authoritative owner of node invariants and behavior:
    /// <list type="bullet">
    /// <item><description>Assigns node identifiers (UUIDv7).</description></item>
    /// <item><description>Enforces kind-specific policies via <see cref="Watson.Nodes.Abstractions.Services.INodeKindHandler"/>.</description></item>
    /// <item><description>Applies attribute patches and increments node versions.</description></item>
    /// <item><description>Implements cursor-based search with projection.</description></item>
    /// </list>
    /// </remarks>
    public interface INodeService
    {
        /// <summary>
        /// Creates a node and persists its attributes.
        /// </summary>
        /// <param name="command">Create command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created node or an error result.</returns>
        Task<Result<Node>> CreateAsync(CreateNodeCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies attribute changes to an existing node.
        /// </summary>
        /// <remarks>
        /// On successful mutation, the node version is incremented.
        /// </remarks>
        /// <param name="nodeId">Target node identifier.</param>
        /// <param name="attributeChanges">Patch command describing attribute upserts/deletes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated node or an error result.</returns>
        Task<Result<Node>> PatchNodeAttributesAsync(Guid nodeId, PatchNodeAttributesCommand attributeChanges, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a node by identifier.
        /// </summary>
        /// <param name="nodeId">Node identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The node or an error result if not found.</returns>
        Task<Result<Node>> GetAsync(Guid nodeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for nodes using cursor-based pagination and returns projected nodes.
        /// </summary>
        /// <remarks>
        /// Search results are cursor-based and exclusive:
        /// the returned results start strictly after the input cursor.
        ///
        /// The returned nodes may be projected (contain only a subset of attributes)
        /// based on requested projection keys or kind defaults.
        /// </remarks>
        /// <param name="query">Search query including filters, cursor, and optional projections.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A cursor result containing nodes and paging metadata; may also represent an error.</returns>
        Task<CursorResult<Node>> SearchAsync(SearchNodesQuery query, CancellationToken cancellationToken = default);
    }
}
