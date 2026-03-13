import type { AttributeFieldDefinition, NodeKindDefinition } from "./definitions";
import {
    AttributeCardinality,
    AttributeValueType,
    type AttributeDto,
    type CreateNodeRequest,
    type NodeDto,
} from "./types";

// Form state: flat key-value map.
// Single-cardinality: string value.
// Many-cardinality: string[] (array of values).
export type FormState = {
    canonicalKey: string;
    scope: string;
    attributes: Record<string, string | string[]>;
};

export type FormErrors = Record<string, string>;

export function buildEmptyForm(definition: NodeKindDefinition): FormState {
    const attributes: Record<string, string | string[]> = {};

    for (const attr of definition.attributes) {
        if (attr.cardinality === AttributeCardinality.Single) {
            attributes[attr.key] = "";
        } else {
            attributes[attr.key] = [];
        }
    }

    return { canonicalKey: "", scope: "", attributes };
}

export function formToCreateRequest(
    definition: NodeKindDefinition,
    form: FormState
): CreateNodeRequest {
    const attrs: AttributeDto[] = [];

    for (const field of definition.attributes) {
        const raw = form.attributes[field.key];
        const values = Array.isArray(raw) ? raw : [raw];

        for (let i = 0; i < values.length; i++) {
            const v = values[i].trim();
            if (v.length === 0) continue;

            attrs.push(buildAttributeDto(field, v, i));
        }
    }

    return {
        canonicalKey: definition.requiresCanonicalKey ? form.canonicalKey.trim() || null : null,
        scope: definition.requiresScope ? form.scope.trim() || null : null,
        attributes: attrs.length > 0 ? attrs : null,
    };
}

export function nodeToFormState(definition: NodeKindDefinition, node: NodeDto): FormState {
    const attributes: Record<string, string | string[]> = {};

    for (const field of definition.attributes) {
        const matching = node.attributes.filter((a) => a.key === field.key);

        if (field.cardinality === AttributeCardinality.Single) {
            attributes[field.key] = matching.length > 0 ? extractValue(field, matching[0]) : "";
        } else {
            attributes[field.key] = matching.map((a) => extractValue(field, a));
        }
    }

    return {
        canonicalKey: node.canonicalKey ?? "",
        scope: node.scope ?? "",
        attributes,
    };
}

export function validateForm(definition: NodeKindDefinition, form: FormState): FormErrors {
    const errors: FormErrors = {};

    if (definition.requiresCanonicalKey && form.canonicalKey.trim().length === 0) {
        errors["canonicalKey"] = "Canonical key is required.";
    }

    if (definition.requiresScope && form.scope.trim().length === 0) {
        errors["scope"] = "Scope is required.";
    }

    for (const field of definition.attributes) {
        if (!field.required) continue;

        const raw = form.attributes[field.key];
        if (Array.isArray(raw)) {
            if (raw.length === 0 || raw.every((v) => v.trim().length === 0)) {
                errors[field.key] = `${field.label} is required.`;
            }
        } else if ((raw ?? "").trim().length === 0) {
            errors[field.key] = `${field.label} is required.`;
        }
    }

    return errors;
}

function buildAttributeDto(
    field: AttributeFieldDefinition,
    value: string,
    position: number
): AttributeDto {
    const base: AttributeDto = {
        key: field.key,
        type: field.type,
        cardinality: field.cardinality,
        position: field.cardinality !== AttributeCardinality.Single ? position : null,
    };

    switch (field.type) {
        case AttributeValueType.String:
            base.valueString = value;
            break;
        case AttributeValueType.Number:
            base.valueNumber = parseFloat(value);
            break;
        case AttributeValueType.Bool:
            base.valueBool = value === "true";
            break;
        case AttributeValueType.Json:
            base.valueJson = value;
            break;
        case AttributeValueType.Ref:
            base.refNodeId = value;
            break;
        case AttributeValueType.Date:
            base.valueDate = value;
            break;
    }

    return base;
}

function extractValue(field: AttributeFieldDefinition, attr: AttributeDto): string {
    switch (field.type) {
        case AttributeValueType.String:
            return attr.valueString ?? "";
        case AttributeValueType.Number:
            return attr.valueNumber != null ? String(attr.valueNumber) : "";
        case AttributeValueType.Bool:
            return attr.valueBool != null ? String(attr.valueBool) : "false";
        case AttributeValueType.Json:
            return attr.valueJson ?? "";
        case AttributeValueType.Ref:
            return attr.refNodeId ?? "";
        case AttributeValueType.Date:
            return attr.valueDate ?? "";
        default:
            return "";
    }
}
