import React from "react";
import { AttributeValueType, type AttributeFieldDefinition } from "@bureau/nodes-core";

type Props = {
    field: AttributeFieldDefinition;
    value: string;
    onChange: (value: string) => void;
    error?: string;
};

export function AttributeField({ field, value, onChange, error }: Props): React.ReactElement {
    const id = `attr-${field.key}`;

    switch (field.type) {
        case AttributeValueType.Bool:
            return (
                <div className="form-field">
                    <label htmlFor={id}>
                        <input
                            id={id}
                            type="checkbox"
                            checked={value === "true"}
                            onChange={(e) => onChange(String(e.target.checked))}
                        />{" "}
                        {field.label}
                    </label>
                    {error && <div className="field-error">{error}</div>}
                </div>
            );

        case AttributeValueType.Number:
            return (
                <div className="form-field">
                    <label htmlFor={id}>{field.label}</label>
                    <input
                        id={id}
                        type="number"
                        value={value}
                        placeholder={field.placeholder}
                        onChange={(e) => onChange(e.target.value)}
                    />
                    {error && <div className="field-error">{error}</div>}
                </div>
            );

        case AttributeValueType.Date:
            return (
                <div className="form-field">
                    <label htmlFor={id}>{field.label}</label>
                    <input
                        id={id}
                        type="date"
                        value={value}
                        onChange={(e) => onChange(e.target.value)}
                    />
                    {error && <div className="field-error">{error}</div>}
                </div>
            );

        case AttributeValueType.Json:
            return (
                <div className="form-field">
                    <label htmlFor={id}>{field.label}</label>
                    <textarea
                        id={id}
                        value={value}
                        placeholder={field.placeholder ?? "JSON"}
                        onChange={(e) => onChange(e.target.value)}
                    />
                    {error && <div className="field-error">{error}</div>}
                </div>
            );

        case AttributeValueType.Ref:
            return (
                <div className="form-field">
                    <label htmlFor={id}>{field.label}</label>
                    <input
                        id={id}
                        type="text"
                        value={value}
                        placeholder={field.placeholder ?? "Node ID (GUID)"}
                        onChange={(e) => onChange(e.target.value)}
                    />
                    {error && <div className="field-error">{error}</div>}
                </div>
            );

        default:
            return (
                <div className="form-field">
                    <label htmlFor={id}>{field.label}</label>
                    <input
                        id={id}
                        type="text"
                        value={value}
                        placeholder={field.placeholder}
                        onChange={(e) => onChange(e.target.value)}
                    />
                    {error && <div className="field-error">{error}</div>}
                </div>
            );
    }
}
