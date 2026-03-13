namespace Watson.Nodes.Abstractions.Conventions
{
    /// <summary>
    /// Defines canonical conventions for representing attribute cardinality and ordering
    /// using <see cref="NodeAttribute.OrderIndex"/>.
    /// </summary>
    /// <remarks>
    /// This convention makes cardinality explicit per stored attribute row using a single integer field:
    /// <list type="bullet">
    /// <item><description><c>-1</c> = single attribute</description></item>
    /// <item><description><c>-2</c> = many unordered (set) attribute element</description></item>
    /// <item><description><c>&gt;= 0</c> = many ordered attribute element (list) with a stable order index</description></item>
    /// </list>
    ///
    /// Normalization and validation are intentionally separated:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Normalization can apply defaults (e.g. set missing OrderIndex based on schema) at API/mapping boundaries.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Validation should be performed in the service layer / handlers to enforce canonical rules.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// This class validates the structural shape of OrderIndex values. It does not enforce per-key rules
    /// such as "brand is single" or "ean is many unordered"—those are handler/schema responsibilities.
    /// </remarks>
    public static class OrderIndexConventions
    {
        /// <summary>
        /// OrderIndex sentinel for a single-valued attribute.
        /// </summary>
        public const int SingleOrderIndex = -1;

        /// <summary>
        /// OrderIndex sentinel for an unordered multi-valued attribute element.
        /// </summary>
        public const int ManyUnorderedOrderIndex = -2;

    }
}
