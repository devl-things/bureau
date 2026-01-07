using Bureau;

namespace Watson.Nodes.Services
{
    /// <summary>
    /// Provides node-kind specific behavior and policy for node operations.
    /// </summary>
    /// <remarks>
    /// Implementations are responsible for:
    /// <list type="bullet">
    /// <item><description>Validating create and patch operations for a specific <see cref="NodeKind"/>.</description></item>
    /// <item><description>Defining default attribute keys used for search matching.</description></item>
    /// <item><description>Defining default attribute keys used for response projection (summary fields).</description></item>
    /// <item><description>Performing optional post-create actions (e.g. writing additional records, links).</description></item>
    /// </list>
    /// Handlers should be lightweight and typically stateless so they can be registered as singletons.
    /// </remarks>
    public interface INodeKindHandler
    {
        /// <summary>
        /// The node kind this handler applies to.
        /// </summary>
        NodeKind Kind { get; }

        /// <summary>
        /// Validates a create request for this node kind.
        /// </summary>
        /// <param name="command">Create command.</param>
        /// <returns>
        /// A successful <see cref="Result"/> when valid; otherwise an error <see cref="Result"/> describing the failure.
        /// </returns>
        Result ValidateCreate(CreateNodeCommand command);

        /// <summary>
        /// Validates an attribute patch request for this node kind.
        /// </summary>
        /// <param name="nodeId">Target node identifier.</param>
        /// <param name="command">Patch command describing attribute changes.</param>
        /// <returns>
        /// A successful <see cref="Result"/> when valid; otherwise an error <see cref="Result"/> describing the failure.
        /// </returns>
        Result ValidatePatch(Guid nodeId, PatchNodeAttributesCommand command);

        /// <summary>
        /// Executes optional post-create logic after the node has been persisted.
        /// </summary>
        /// <remarks>
        /// Use this for kind-specific side effects such as writing additional records or link tables.
        /// For most kinds this is a no-op.
        /// </remarks>
        /// <param name="node">The created node (domain).</param>
        /// <param name="command">The create command that produced the node.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A successful <see cref="Result"/> when completed; otherwise an error <see cref="Result"/> describing the failure.
        /// </returns>
        Task<Result> AfterCreateAsync(Node node, CreateNodeCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the default attribute keys that are searched when a caller provides a query text
        /// but does not specify query attribute keys explicitly.
        /// </summary>
        /// <remarks>
        /// These keys are used for matching/search filtering (not projection). Typically includes keys like
        /// <c>label</c> and optionally <c>description</c>.
        /// </remarks>
        IReadOnlyList<NodeAttributeKey> GetSearchAttributeKeys();

        /// <summary>
        /// Returns the default attribute keys included in projected search responses when a caller does not
        /// specify projection keys explicitly.
        /// </summary>
        /// <remarks>
        /// These keys affect the response payload only and do not influence search matching.
        /// </remarks>
        IReadOnlyList<NodeAttributeKey> GetSummaryAttributeKeys();
    }
}
