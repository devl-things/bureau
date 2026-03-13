using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Constants;

namespace Watson.Nodes.Definitions
{
    internal static class VariantDefinition
    {
        public static readonly List<string> VariantSearchKeys = [NodeAttributeKeys.Gtin, NodeAttributeKeys.Ean, NodeAttributeKeys.Upc, NodeAttributeKeys.Barcode];
        public static readonly List<string> VariantSummaryKeys = [NodeAttributeKeys.Brand, .. VariantSearchKeys];

        private static readonly NodeAttributeDefinition _itemDefinition = new()
        {
            Key = NodeAttributeKeys.Item,
            Type = AttributeValueType.Ref,
            IsRequired = true,
            OrderIndex = OrderIndexConventions.SingleOrderIndex
        };
        private static readonly NodeAttributeDefinition _gtinDefinition = new()
        {
            Key = NodeAttributeKeys.Gtin,
            Type = AttributeValueType.String,
            IsRequired = false,
            OrderIndex = OrderIndexConventions.SingleOrderIndex
        };

        private static readonly NodeAttributeDefinition _barcodeDefinition = new()
        {
            Key = NodeAttributeKeys.Barcode,
            Type = AttributeValueType.String,
            IsRequired = false,
            OrderIndex = OrderIndexConventions.SingleOrderIndex
        };

        private static readonly NodeAttributeDefinition _eanDefinition = new()
        {
            Key = NodeAttributeKeys.Ean,
            Type = AttributeValueType.String,
            IsRequired = false,
            OrderIndex = OrderIndexConventions.ManyUnorderedOrderIndex
        };

        private static readonly NodeAttributeDefinition _upcDefinition = new()
        {
            Key = NodeAttributeKeys.Upc,
            Type = AttributeValueType.String,
            IsRequired = false,
            OrderIndex = OrderIndexConventions.ManyUnorderedOrderIndex
        };
        private static readonly NodeAttributeDefinition _tagDefinition = new()
        {
            Key = NodeAttributeKeys.Tag,
            Type = AttributeValueType.Ref,
            IsRequired = false,
            OrderIndex = OrderIndexConventions.ManyUnorderedOrderIndex
        };
        private static readonly NodeAttributeDefinition _brandDefinition = new()
        {
            Key = NodeAttributeKeys.Brand,
            Type = AttributeValueType.Ref,
            IsRequired = false,
            OrderIndex = OrderIndexConventions.SingleOrderIndex
        };

        public static readonly Dictionary<string, NodeAttributeDefinition> AttributeDefinitions = new()
        {
            { NodeAttributeKeys.Item, _itemDefinition},
            // Identifiers
            { NodeAttributeKeys.Gtin,_gtinDefinition},
            { NodeAttributeKeys.Barcode, _barcodeDefinition},
            // Multi identifiers
            { NodeAttributeKeys.Ean, _eanDefinition },
            { NodeAttributeKeys.Upc, _upcDefinition },
            // Tagging
            { NodeAttributeKeys.Tag, _tagDefinition },
            // Brand is a relationship (Ref), not text
            { NodeAttributeKeys.Brand, _brandDefinition },
        };

        public static Dictionary<string, bool> GetRequiredTemplate()
        {
            return new Dictionary<string, bool>
            {
                { NodeAttributeKeys.Item, false },
            };
        }
    }
}
