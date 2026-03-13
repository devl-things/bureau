using Watson.Nodes.Models;

namespace Watson.Nodes.Mappers
{
    internal static class NodeMapperExtension
    {
        public static Node ToDomain(this NodeDb db, List<NodeAttributeDb>? attributes)
        {
            Node node = new Node(db.CanonicalKey ?? string.Empty, db.Version, db.Status)
            {
                NodeId = db.NodeId,
                Kind = db.Kind,
                Scope = db.Scope,
            };
            if (attributes != null)
            {
                node.SetAttributes(attributes.Select(x => x.ToDomain()));
            }
            return node;
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
