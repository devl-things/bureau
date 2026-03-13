import React, { useState } from "react";
import {
    type NodeKindDefinition,
    type NodesApi,
    type FormState,
    type FormErrors,
    buildEmptyForm,
    formToCreateRequest,
    validateForm,
} from "@bureau/nodes-core";
import { NodeForm } from "../components/NodeForm";

type Props = {
    definition: NodeKindDefinition;
    nodesApi: NodesApi;
    onCreated: () => void;
    onCancel: () => void;
};

export function NodeCreatePage({ definition, nodesApi, onCreated, onCancel }: Props): React.ReactElement {
    const [form, setForm] = useState<FormState>(() => buildEmptyForm(definition));
    const [errors, setErrors] = useState<FormErrors>({});
    const [submitting, setSubmitting] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);

    async function handleSubmit(e: React.FormEvent): Promise<void> {
        e.preventDefault();

        const validation = validateForm(definition, form);
        if (Object.keys(validation).length > 0) {
            setErrors(validation);
            return;
        }

        setErrors({});
        setApiError(null);
        setSubmitting(true);

        try {
            const request = formToCreateRequest(definition, form);
            await nodesApi.createNode(definition, request);
            onCreated();
        } catch (err) {
            setApiError(err instanceof Error ? err.message : "Failed to create node.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div>
            <div className="breadcrumbs" style={{ marginBottom: 12 }}>
                <button type="button" className="btn ghost" onClick={onCancel}>
                    &larr; Back to {definition.pluralLabel}
                </button>
            </div>

            <div className="panel" style={{ padding: 18 }}>
                <h2 style={{ margin: "0 0 12px", fontSize: "1.05rem", fontWeight: 600 }}>
                    Create {definition.label}
                </h2>

                <form onSubmit={(e) => void handleSubmit(e)}>
                    <NodeForm
                        definition={definition}
                        form={form}
                        errors={errors}
                        onChange={setForm}
                    />

                    {apiError && (
                        <div className="field-error" style={{ marginTop: 8 }}>
                            {apiError}
                        </div>
                    )}

                    <div className="modal-actions" style={{ marginTop: 12 }}>
                        <button type="button" className="btn ghost" onClick={onCancel}>
                            Cancel
                        </button>
                        <button type="submit" className="btn" disabled={submitting}>
                            {submitting ? "Creating..." : `Create ${definition.label}`}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
