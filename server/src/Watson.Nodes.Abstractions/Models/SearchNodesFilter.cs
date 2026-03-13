namespace Watson.Nodes
{
    /// <summary>
    /// Domain filter for searching nodes.
    /// </summary>
    /// <remarks>
    /// The search operation may return projected nodes where only a subset of attributes are loaded,
    /// based on <see cref="AttributeKeys"/> or handler defaults for the node kind.
    /// </remarks>
    public sealed class SearchNodesFilter
    {
        /// <summary>
        /// Node kind to search within (e.g., Item, Tag).
        /// </summary>
        public NodeKind Kind { get; init; }

        /// <summary>
        /// Free-text query used for searching nodes (typically applied to the localized label or other attributes).
        /// </summary>
        public string? Query { get; init; }

        /// <summary>
        /// Attribute keys that should be searched when applying the <see cref="Query"/> filter.
        /// </summary>
        /// <remarks>
        /// When specified, the search operation evaluates the query text only against attributes
        /// whose keys are listed here (typically string-valued attributes such as <c>label</c>
        /// or <c>description</c>).
        ///
        /// When null or empty:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// The service uses a default set of searchable attribute keys defined per <see cref="NodeKind"/>.
        /// </description>
        /// </item>
        /// </list>
        ///
        /// Only string-valued attributes are considered for text search unless explicitly documented otherwise.
        /// The interpretation of search semantics (e.g. contains vs. prefix) is defined by the service layer.
        /// </remarks>
        public IReadOnlyList<string>? QueryAttributeKeys { get; init; }

        /// <summary>
        /// Optional scope filter. Null means any scope.
        /// </summary>
        /// <remarks>
        /// Use <c>global</c> for global nodes. Other scopes should follow a consistent naming convention
        /// (e.g., <c>shoppinglist</c>, <c>chores</c>, <c>tenant:acme</c>).
        /// </remarks>
        public string? Scope { get; init; }

        /// <summary>
        /// Optional locale preference expressed as a BCP 47 language tag.
        /// </summary>
        /// <remarks>
        /// When specified, locale-aware attributes (e.g., <c>label</c>) should prefer this locale.
        /// Null means no locale preference.
        /// </remarks>
        public string? Locale { get; init; }

        /// <summary>
        /// Optional list of attribute keys to project into the returned nodes.
        /// </summary>
        /// <remarks>
        /// When null or empty, the system uses a default projection defined per node kind.
        /// Keys should be lowercase snake_case and may be namespaced (e.g., <c>catalog:ean</c>).
        /// </remarks>
        public IReadOnlyList<string>? AttributeKeys { get; init; }

        /// <summary>
        /// Creates an empty filter instance. Properties are expected to be set via object initializer.
        /// </summary>
        public SearchNodesFilter()
        {
        }
    }
}
