import React from "react";
import type { NodeDto, NodeKindDefinition } from "@bureau/nodes-core";

type Props = {
    definition: NodeKindDefinition;
    nodes: NodeDto[];
    onSelect: (node: NodeDto) => void;
    hasMore: boolean;
    onLoadMore: () => void;
    loading: boolean;
};

function getDisplayValue(node: NodeDto, key: string): string {
    const attr = node.attributes.find((a) => a.key === key);
    if (!attr) return "";
    return attr.valueString ?? attr.refNodeId ?? attr.valueDate ?? String(attr.valueNumber ?? attr.valueBool ?? "");
}

export function NodeTable({ definition, nodes, onSelect, hasMore, onLoadMore, loading }: Props): React.ReactElement {
    if (nodes.length === 0 && !loading) {
        return <div className="empty">No {definition.pluralLabel.toLowerCase()} found.</div>;
    }

    const labelAttr = definition.attributes.find((a) => a.key === "label");
    const showLabel = !!labelAttr;

    return (
        <div className="panel">
            <table>
                <thead>
                    <tr>
                        {definition.requiresCanonicalKey && <th>Key</th>}
                        {definition.requiresScope && <th>Scope</th>}
                        {showLabel && <th>Label</th>}
                        <th>Node ID</th>
                        <th>Version</th>
                    </tr>
                </thead>
                <tbody>
                    {nodes.map((node) => (
                        <tr
                            key={node.nodeId}
                            style={{ cursor: "pointer" }}
                            onClick={() => onSelect(node)}
                        >
                            {definition.requiresCanonicalKey && <td>{node.canonicalKey}</td>}
                            {definition.requiresScope && <td>{node.scope}</td>}
                            {showLabel && <td>{getDisplayValue(node, "label")}</td>}
                            <td style={{ fontFamily: "monospace", fontSize: "0.85rem" }}>{node.nodeId}</td>
                            <td>{node.version}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {hasMore && (
                <div style={{ padding: "12px 20px" }}>
                    <button
                        type="button"
                        className="btn ghost"
                        disabled={loading}
                        onClick={onLoadMore}
                    >
                        {loading ? "Loading..." : "Load more"}
                    </button>
                </div>
            )}
        </div>
    );
}
