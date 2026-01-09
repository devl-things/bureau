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
        /// Returns the default set of attribute keys used when searching nodes of this kind.
        /// </summary>
        /// <remarks>
        /// These keys are used when the caller does not explicitly specify which attributes
        /// should participate in search matching (e.g. label, description).
        ///
        /// The returned keys represent the canonical, handler-defined defaults for this
        /// node kind and should be stable over time.
        /// </remarks>
        /// <returns>
        /// A list of attribute keys that are searched by default.
        /// </returns>
        List<string> GetDefaultSearchAttributeKeys();

        /// <summary>
        /// Resolves the set of attribute keys used for search matching.
        /// </summary>
        /// <remarks>
        /// If <paramref name="attributeKeys"/> is provided and contains at least one value,
        /// it is treated as an explicit override supplied by the caller.
        ///
        /// If <paramref name="attributeKeys"/> is <c>null</c> or empty, the handler-defined
        /// default search attribute keys are used instead.
        /// </remarks>
        /// <param name="attributeKeys">
        /// Optional list of attribute keys requested by the caller.
        /// </param>
        /// <returns>
        /// A distinct list of attribute keys to be used for search matching.
        /// </returns>
        List<string> GetSearchAttributeKeys(IReadOnlyList<string>? attributeKeys);

        /// <summary>
        /// Returns the default set of attribute keys included when projecting node summaries.
        /// </summary>
        /// <remarks>
        /// These keys define the minimal attribute set returned by default in search
        /// and listing scenarios (e.g. label, description).
        ///
        /// The returned keys are handler-defined and represent the canonical summary
        /// projection for this node kind.
        /// </remarks>
        /// <returns>
        /// A list of attribute keys included in default node projections.
        /// </returns>
        List<string> GetDefaultSummaryAttributeKeys();

        /// <summary>
        /// Resolves the set of attribute keys included in the node projection.
        /// </summary>
        /// <remarks>
        /// If <paramref name="attributeKeys"/> is provided and contains at least one value,
        /// it is treated as an explicit projection requested by the caller.
        ///
        /// If <paramref name="attributeKeys"/> is <c>null</c> or empty, the handler-defined
        /// default summary attribute keys are used instead.
        /// </remarks>
        /// <param name="attributeKeys">
        /// Optional list of attribute keys requested by the caller.
        /// </param>
        /// <returns>
        /// A distinct list of attribute keys to include in the node projection.
        /// </returns>
        List<string> GetProjectionAttributeKeys(IReadOnlyList<string>? attributeKeys);
    }
}
