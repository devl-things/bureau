using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Mappers
{
    public static class EnumsMapperExtension
    {
        public static NodeKind ToDomain(this NodeKindContract kind)
        {
            return (NodeKind)kind;
        }
        public static NodeKindContract ToContract(this NodeKind kind)
        {
            return (NodeKindContract)kind;
        }
        public static NodeStatus ToDomain(this NodeStatusContract status)
        {
            return (NodeStatus)status;
        }
        public static NodeStatusContract ToContract(this NodeStatus status)
        {
            return (NodeStatusContract)status;
        }
        public static AttributeValueType ToDomain(this AttributeValueTypeContract type)
        {
            return (AttributeValueType)type;
        }

        public static AttributeValueTypeContract ToContract(this AttributeValueType type)
        {
            return (AttributeValueTypeContract)type;
        }

        public static ChangeMode ToDomain(this ChangeModeContract mode)
        {
            return (ChangeMode)mode;
        }

        public static ChangeModeContract ToContract(this ChangeMode mode)
        {
            return (ChangeModeContract)mode;
        }
    }
}
