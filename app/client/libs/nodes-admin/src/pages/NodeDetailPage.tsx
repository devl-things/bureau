import React, { useState } from "react";
import {
    type NodeDto,
    type NodeKindDefinition,
    type NodesApi,
    type FormState,
    type FormErrors,
    nodeToFormState,
    formToCreateRequest,
    validateForm,
} from "@bureau/nodes-core";
import { NodeForm } from "../components/NodeForm";
import { EdgeList } from "../components/EdgeList";

type Props = {
    node: NodeDto;
    definition: NodeKindDefinition;
    nodesApi: NodesApi;
    onBack: () => void;
};

export function NodeDetailPage({ node, definition, nodesApi, onBack }: Props): React.ReactElement {
    const [form, setForm] = useState<FormState>(() => nodeToFormState(definition, node));
    const [errors, setErrors] = useState<FormErrors>({});
    const [saving, setSaving] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);
    const [saved, setSaved] = useState(false);

    async function handleSave(e: React.FormEvent): Promise<void> {
        e.preventDefault();

        const validation = validateForm(definition, form);
        if (Object.keys(validation).length > 0) {
            setErrors(validation);
            return;
        }

        setErrors({});
        setApiError(null);
        setSaving(true);
        setSaved(false);

        try {
            const request = formToCreateRequest(definition, form);
            await nodesApi.patchAttributes(node.nodeId, {
                set: request.attributes,
            });
            setSaved(true);
        } catch (err) {
            setApiError(err instanceof Error ? err.message : "Failed to save.");
        } finally {
            setSaving(false);
        }
    }

    return (
        <div>
            <div className="breadcrumbs" style={{ marginBottom: 12 }}>
                <button type="button" className="btn ghost" onClick={onBack}>
                    &larr; Back to {definition.pluralLabel}
                </button>
            </div>

            <div className="panel" style={{ padding: 18 }}>
                <h2 style={{ margin: "0 0 4px", fontSize: "1.05rem", fontWeight: 600 }}>
                    {definition.label} Detail
                </h2>
                <div className="meta-line" style={{ marginBottom: 14 }}>
                    <span className="muted-label">ID:</span>{" "}
                    <span style={{ fontFamily: "monospace", fontSize: "0.85rem" }}>{node.nodeId}</span>
                    <span className="muted-label" style={{ marginLeft: 12 }}>Version:</span> {node.version}
                </div>

                <form onSubmit={(e) => void handleSave(e)}>
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

                    {saved && (
                        <div style={{ color: "var(--accent)", marginTop: 8, fontSize: "0.9rem" }}>
                            Saved successfully.
                        </div>
                    )}

                    <div className="modal-actions" style={{ marginTop: 12 }}>
                        <button type="button" className="btn ghost" onClick={onBack}>
                            Cancel
                        </button>
                        <button type="submit" className="btn" disabled={saving}>
                            {saving ? "Saving..." : "Save Changes"}
                        </button>
                    </div>
                </form>
            </div>

            <div className="panel" style={{ padding: 18, marginTop: 14 }}>
                <EdgeList nodeId={node.nodeId} nodesApi={nodesApi} />
            </div>
        </div>
    );
}
