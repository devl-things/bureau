using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Mappers
{
    public static class NodeMapperExtension
    {
        public static CreateNodeCommand ToDomain(this CreateNodeRequest request, NodeKind kind)
        {
            return new CreateNodeCommand
            {
                Kind = kind,
                Scope = ScopeConventions.Normalize(request.Scope),
                CanonicalKey = CanonicalKeyConventions.Normalize(request.CanonicalKey),
                Attributes = [.. (request.Attributes ?? Array.Empty<AttributeDto>()).Select(a => a.ToDomain())]
            };
        }

        public static NodeDto ToDto(this Node node)
        {
            return new NodeDto
            {
                NodeId = node.NodeId,
                Kind = node.Kind.ToContractKind(),
                Scope = node.Scope,
                CanonicalKey = node.CanonicalKey,
                Status = node.Status.ToContractStatus(),
                Version = node.Version,
                Attributes = node.Attributes.Select(a => a.ToDto()).ToList()
            };
        }
    }
}
