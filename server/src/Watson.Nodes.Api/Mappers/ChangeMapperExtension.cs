using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Mappers
{
    public static class ChangeMapperExtension
    {
        public static ChangeEventDto ToDto(this ChangeEvent e)
        {
            ChangePayloadDto payload = new ChangePayloadDto
            {
                Scope = e.Payload.Scope,
                CanonicalKey = e.Payload.CanonicalKey,
                Status = e.Payload.Status.ToContract(),
                Attributes = e.Payload.Attributes is null ? null : e.Payload.Attributes.Select(a => a.ToDto()).ToList()
            };

            return new ChangeEventDto
            {
                Sequence = e.Sequence,
                Type = e.Type,
                Kind = e.Kind.ToContract(),
                NodeId = e.NodeId,
                Version = e.Version,
                Payload = payload
            };
        }
    }
}
