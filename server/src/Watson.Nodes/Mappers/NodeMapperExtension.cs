using Watson.Nodes.Models;

namespace Watson.Nodes.Mappers
{
    internal static class NodeMapperExtension
    {
        public static Node ToDomain(this NodeDb db, IReadOnlyList<NodeAttributeDb> attributes)
        {
            IReadOnlyCollection<NodeAttribute> domainAttributes =
                attributes.Select(x => x.ToDomain()).ToList();
            return new Node()
            {
                NodeId = db.NodeId,
                Kind = db.Kind,
                Scope = db.Scope,
                //TODO
                //CanonicalKey = db.CanonicalKey ?? string.Empty,
                //Status = db.Status,
                //Version = db.Version,
                //Attributes = domainAttributes
            };
        }

        public static NodeAttribute ToDomain(this NodeAttributeDb db)
        {
            return new NodeAttribute(db.Key)
            {
                Locale = db.Locale,
                Type = db.Type,
                ValueString = db.ValueString,
                ValueNumber = db.ValueNumber,
                ValueBool = db.ValueBool,
                ValueJson = db.ValueJson,
                RefNodeId = db.RefNodeId,
                ValueDate = db.ValueDate
            };
        }
    }
}
