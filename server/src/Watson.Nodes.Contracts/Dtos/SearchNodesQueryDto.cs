namespace Watson.Nodes.Contracts.Dtos
{
    /// <summary>
    /// Query parameters for searching nodes using cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// This query supports:
    /// <list type="bullet">
    /// <item><description>cursor-based pagination</description></item>
    /// <item><description>free-text search</description></item>
    /// <item><description>attribute projection</description></item>
    /// <item><description>locale-aware filtering</description></item>
    /// </list>
    /// </remarks>
    public sealed class SearchNodesQueryDto : CursorQueryDto
    {
        /// <summary>
        /// Free-text search query applied to node attributes.
        /// </summary>
        /// <remarks>
        /// When specified, the service evaluates this query against selected string-valued
        /// attributes (see <see cref="QueryAttributes"/>).
        ///
        /// When null or empty, no text search is performed and all matching nodes are eligible
        /// for inclusion based on other filters.
        /// </remarks>
        public string? Query { get; set; }

        /// <summary>
        /// Attribute keys that should be searched when applying <see cref="Query"/>.
        /// </summary>
        /// <remarks>
        /// This value is a comma-separated list of attribute keys.
        ///
        /// When specified, the search operation evaluates the query text only against
        /// the listed attribute keys.
        ///
        /// When omitted or empty, the service uses a default set of searchable attributes
        /// defined per node kind (for example, <c>label</c> or <c>description</c>).
        ///
        /// Only string-valued attributes are considered for text search.
        /// </remarks>
        public string? QueryAttributes { get; set; }

        /// <summary>
        /// Optional scope filter applied to the search.
        /// </summary>
        /// <remarks>
        /// When specified, only nodes within the given scope are considered.
        /// When null, nodes from any scope may be returned.
        /// </remarks>
        public string? Scope { get; set; }

        /// <summary>
        /// Optional locale preference for locale-aware attributes.
        /// </summary>
        /// <remarks>
        /// When specified, the service prefers attribute values with the given locale.
        /// Fallback behavior (e.g. invariant or default locale) is defined by the service.
        /// </remarks>
        public string? Locale { get; set; }

        /// <summary>
        /// Attribute keys to include in the search response.
        /// </summary>
        /// <remarks>
        /// This controls attribute projection in the response.
        ///
        /// When specified, only attributes with the listed keys are included in returned nodes.
        /// When null or empty, the service uses a default projection defined per node kind.
        ///
        /// Projection affects the response payload only and does not influence search matching.
        /// </remarks>
        public IReadOnlyList<string>? Attributes { get; set; }
    }
}
