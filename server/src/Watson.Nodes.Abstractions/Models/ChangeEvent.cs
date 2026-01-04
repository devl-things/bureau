namespace Watson.Nodes
{
    public sealed class ChangeEvent
    {
        public long Sequence { get; }
        public string Type { get; }
        public NodeKind Kind { get; }
        public Guid NodeId { get; }
        public int Version { get; }
        public ChangePayload Payload { get; }

        public ChangeEvent(long sequence, string type, NodeKind kind, Guid nodeId, int version, ChangePayload payload)
        {
            Sequence = sequence;
            Type = type;
            Kind = kind;
            NodeId = nodeId;
            Version = version;
            Payload = payload;
        }
    }
}
