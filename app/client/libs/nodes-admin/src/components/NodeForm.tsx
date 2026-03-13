import React from "react";
import {
    AttributeCardinality,
    type NodeKindDefinition,
    type FormState,
    type FormErrors,
} from "@bureau/nodes-core";
import { AttributeField } from "./AttributeField";

type Props = {
    definition: NodeKindDefinition;
    form: FormState;
    errors: FormErrors;
    onChange: (form: FormState) => void;
};

export function NodeForm({ definition, form, errors, onChange }: Props): React.ReactElement {
    function setCanonicalKey(value: string): void {
        onChange({ ...form, canonicalKey: value });
    }

    function setScope(value: string): void {
        onChange({ ...form, scope: value });
    }

    function setSingleAttribute(key: string, value: string): void {
        onChange({
            ...form,
            attributes: { ...form.attributes, [key]: value },
        });
    }

    function setManyAttribute(key: string, index: number, value: string): void {
        const current = form.attributes[key];
        const arr = Array.isArray(current) ? [...current] : [];
        arr[index] = value;
        onChange({
            ...form,
            attributes: { ...form.attributes, [key]: arr },
        });
    }

    function addManyEntry(key: string): void {
        const current = form.attributes[key];
        const arr = Array.isArray(current) ? [...current, ""] : [""];
        onChange({
            ...form,
            attributes: { ...form.attributes, [key]: arr },
        });
    }

    function removeManyEntry(key: string, index: number): void {
        const current = form.attributes[key];
        if (!Array.isArray(current)) return;
        const arr = current.filter((_, i) => i !== index);
        onChange({
            ...form,
            attributes: { ...form.attributes, [key]: arr },
        });
    }

    return (
        <div className="node-form">
            {definition.requiresCanonicalKey && (
                <div className="form-field">
                    <label htmlFor="canonicalKey">Canonical Key</label>
                    <input
                        id="canonicalKey"
                        type="text"
                        value={form.canonicalKey}
                        placeholder="Unique identifier"
                        onChange={(e) => setCanonicalKey(e.target.value)}
                    />
                    {errors["canonicalKey"] && (
                        <div className="field-error">{errors["canonicalKey"]}</div>
                    )}
                </div>
            )}

            {definition.requiresScope && (
                <div className="form-field">
                    <label htmlFor="scope">Scope</label>
                    <input
                        id="scope"
                        type="text"
                        value={form.scope}
                        placeholder="Scope"
                        onChange={(e) => setScope(e.target.value)}
                    />
                    {errors["scope"] && <div className="field-error">{errors["scope"]}</div>}
                </div>
            )}

            {definition.attributes.map((field) => {
                if (field.cardinality === AttributeCardinality.Single) {
                    const value = form.attributes[field.key];
                    return (
                        <AttributeField
                            key={field.key}
                            field={field}
                            value={typeof value === "string" ? value : ""}
                            onChange={(v) => setSingleAttribute(field.key, v)}
                            error={errors[field.key]}
                        />
                    );
                }

                // Many cardinality
                const values = Array.isArray(form.attributes[field.key])
                    ? (form.attributes[field.key] as string[])
                    : [];

                return (
                    <div key={field.key} className="form-field">
                        <label>{field.label}</label>
                        {values.map((val, i) => (
                            <div key={i} style={{ display: "flex", gap: 6, marginBottom: 4 }}>
                                <AttributeField
                                    field={field}
                                    value={val}
                                    onChange={(v) => setManyAttribute(field.key, i, v)}
                                />
                                <button
                                    type="button"
                                    className="btn btn-danger"
                                    onClick={() => removeManyEntry(field.key, i)}
                                >
                                    Remove
                                </button>
                            </div>
                        ))}
                        <button
                            type="button"
                            className="btn ghost"
                            onClick={() => addManyEntry(field.key)}
                        >
                            + Add {field.label}
                        </button>
                        {errors[field.key] && (
                            <div className="field-error">{errors[field.key]}</div>
                        )}
                    </div>
                );
            })}
        </div>
    );
}
