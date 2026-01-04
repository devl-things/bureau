namespace Watson.Nodes
{
    public sealed class NodeAttribute
    {
        public string Key { get; }
        public string? Locale { get; }
        public AttributeValueType Type { get; }

        public string? ValueString { get; }
        public decimal? ValueNumber { get; }
        public bool? ValueBool { get; }
        public string? ValueJson { get; }
        public Guid? RefNodeId { get; }
        public DateTimeOffset? ValueDate { get; }

        public NodeAttribute(
            string key,
            string? locale,
            AttributeValueType type,
            string? valueString,
            decimal? valueNumber,
            bool? valueBool,
            string? valueJson,
            Guid? refNodeId,
            DateTimeOffset? valueDate)
        {
            Key = key;
            Locale = locale;
            Type = type;
            ValueString = valueString;
            ValueNumber = valueNumber;
            ValueBool = valueBool;
            ValueJson = valueJson;
            RefNodeId = refNodeId;
            ValueDate = valueDate;
        }
    }
}
