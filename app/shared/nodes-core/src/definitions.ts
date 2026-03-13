import { AttributeCardinality, AttributeValueType, NodeKind } from "./types";

export type AttributeFieldDefinition = {
    key: string;
    label: string;
    type: AttributeValueType;
    required: boolean;
    cardinality: AttributeCardinality;
    placeholder?: string;
};

export type NodeKindDefinition = {
    kind: NodeKind;
    label: string;
    pluralLabel: string;
    apiRoute: string;
    requiresCanonicalKey: boolean;
    requiresScope: boolean;
    attributes: AttributeFieldDefinition[];
};

const commonLabelDescription: AttributeFieldDefinition[] = [
    {
        key: "label",
        label: "Label",
        type: AttributeValueType.String,
        required: true,
        cardinality: AttributeCardinality.Single,
        placeholder: "Display name",
    },
    {
        key: "description",
        label: "Description",
        type: AttributeValueType.String,
        required: false,
        cardinality: AttributeCardinality.Single,
        placeholder: "Optional description",
    },
];

export const ItemDefinition: NodeKindDefinition = {
    kind: NodeKind.Item,
    label: "Item",
    pluralLabel: "Items",
    apiRoute: "items",
    requiresCanonicalKey: true,
    requiresScope: true,
    attributes: commonLabelDescription,
};

export const TagDefinition: NodeKindDefinition = {
    kind: NodeKind.Tag,
    label: "Tag",
    pluralLabel: "Tags",
    apiRoute: "tags",
    requiresCanonicalKey: true,
    requiresScope: true,
    attributes: commonLabelDescription,
};

export const ProjectDefinition: NodeKindDefinition = {
    kind: NodeKind.Project,
    label: "Project",
    pluralLabel: "Projects",
    apiRoute: "projects",
    requiresCanonicalKey: false,
    requiresScope: false,
    attributes: commonLabelDescription,
};

export const VariantDefinition: NodeKindDefinition = {
    kind: NodeKind.Variant,
    label: "Variant",
    pluralLabel: "Variants",
    apiRoute: "variants",
    requiresCanonicalKey: false,
    requiresScope: false,
    attributes: [
        {
            key: "item",
            label: "Item",
            type: AttributeValueType.Ref,
            required: true,
            cardinality: AttributeCardinality.Single,
            placeholder: "Item node ID",
        },
        {
            key: "gtin",
            label: "GTIN",
            type: AttributeValueType.String,
            required: false,
            cardinality: AttributeCardinality.Single,
            placeholder: "Global Trade Item Number",
        },
        {
            key: "barcode",
            label: "Barcode",
            type: AttributeValueType.String,
            required: false,
            cardinality: AttributeCardinality.Single,
            placeholder: "Barcode value",
        },
        {
            key: "ean",
            label: "EAN",
            type: AttributeValueType.String,
            required: false,
            cardinality: AttributeCardinality.ManyUnordered,
            placeholder: "European Article Number",
        },
        {
            key: "upc",
            label: "UPC",
            type: AttributeValueType.String,
            required: false,
            cardinality: AttributeCardinality.ManyUnordered,
            placeholder: "Universal Product Code",
        },
        {
            key: "tag",
            label: "Tag",
            type: AttributeValueType.Ref,
            required: false,
            cardinality: AttributeCardinality.ManyUnordered,
            placeholder: "Tag node ID",
        },
        {
            key: "brand",
            label: "Brand",
            type: AttributeValueType.Ref,
            required: false,
            cardinality: AttributeCardinality.Single,
            placeholder: "Brand node ID",
        },
    ],
};

export const AllDefinitions: NodeKindDefinition[] = [
    ItemDefinition,
    TagDefinition,
    VariantDefinition,
    ProjectDefinition,
];

export function getDefinition(kind: NodeKind): NodeKindDefinition {
    const def = AllDefinitions.find((d) => d.kind === kind);
    if (!def) {
        throw new Error(`No definition for NodeKind ${kind}`);
    }
    return def;
}
