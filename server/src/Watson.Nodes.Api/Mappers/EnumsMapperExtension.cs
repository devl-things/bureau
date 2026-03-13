using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Mappers
{
    public static class EnumsMapperExtension
    {
        public static NodeKind ToDomainKind(this NodeKindContract kind)
        {
            return (NodeKind)kind;
        }
        public static NodeKindContract ToContractKind(this NodeKind kind)
        {
            return (NodeKindContract)kind;
        }
        public static NodeStatus ToDomainStatus(this NodeStatusContract status)
        {
            return (NodeStatus)status;
        }
        public static NodeStatusContract ToContractStatus(this NodeStatus status)
        {
            return (NodeStatusContract)status;
        }
        public static AttributeValueType ToDomainValueType(this AttributeValueTypeContract type)
        {
            return (AttributeValueType)type;
        }

        public static AttributeValueTypeContract ToContractValueType(this AttributeValueType type)
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
