import React, { useCallback, useEffect, useState } from "react";
import type { EdgeDto, NodesApi } from "@bureau/nodes-core";

type Props = {
    nodeId: string;
    nodesApi: NodesApi;
};

export function EdgeList({ nodeId, nodesApi }: Props): React.ReactElement {
    const [edges, setEdges] = useState<EdgeDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [purpose, setPurpose] = useState("");
    const [targetNodeId, setTargetNodeId] = useState("");
    const [orderIndex, setOrderIndex] = useState("0");
    const [error, setError] = useState<string | null>(null);

    const loadEdges = useCallback(async () => {
        setLoading(true);
        try {
            const result = await nodesApi.getEdges(nodeId);
            setEdges(result.data);
        } catch {
            setEdges([]);
        } finally {
            setLoading(false);
        }
    }, [nodeId, nodesApi]);

    useEffect(() => {
        void loadEdges();
    }, [loadEdges]);

    async function handleAdd(): Promise<void> {
        if (!targetNodeId.trim() || !purpose.trim()) {
            setError("Target node ID and purpose are required.");
            return;
        }
        setError(null);
        try {
            await nodesApi.addEdge(nodeId, {
                targetNodeId: targetNodeId.trim(),
                purpose: purpose.trim(),
                orderIndex: parseInt(orderIndex, 10) || 0,
            });
            setTargetNodeId("");
            setPurpose("");
            setOrderIndex("0");
            await loadEdges();
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to add edge.");
        }
    }

    async function handleRemove(edge: EdgeDto): Promise<void> {
        try {
            await nodesApi.removeEdge(nodeId, {
                targetNodeId: edge.targetNodeId,
                purpose: edge.purpose,
            });
            await loadEdges();
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to remove edge.");
        }
    }

    return (
        <div className="edge-list">
            <h3>Edges</h3>

            {loading && <div className="muted-label">Loading edges...</div>}

            {!loading && edges.length === 0 && (
                <div className="muted-label">No edges.</div>
            )}

            {edges.length > 0 && (
                <table>
                    <thead>
                        <tr>
                            <th>Target</th>
                            <th>Purpose</th>
                            <th>Order</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {edges.map((edge, i) => (
                            <tr key={`${edge.targetNodeId}-${edge.purpose}-${i}`}>
                                <td style={{ fontFamily: "monospace", fontSize: "0.85rem" }}>{edge.targetNodeId}</td>
                                <td>{edge.purpose}</td>
                                <td>{edge.orderIndex}</td>
                                <td>
                                    <button
                                        type="button"
                                        className="btn btn-danger"
                                        onClick={() => void handleRemove(edge)}
                                    >
                                        Remove
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            <div className="edge-add-form" style={{ display: "flex", gap: 8, flexWrap: "wrap", marginTop: 10 }}>
                <input
                    type="text"
                    placeholder="Target node ID"
                    value={targetNodeId}
                    onChange={(e) => setTargetNodeId(e.target.value)}
                />
                <input
                    type="text"
                    placeholder="Purpose"
                    value={purpose}
                    onChange={(e) => setPurpose(e.target.value)}
                />
                <input
                    type="number"
                    placeholder="Order"
                    value={orderIndex}
                    onChange={(e) => setOrderIndex(e.target.value)}
                    style={{ width: 80 }}
                />
                <button type="button" className="btn" onClick={() => void handleAdd()}>
                    Add Edge
                </button>
            </div>

            {error && <div className="field-error" style={{ marginTop: 6 }}>{error}</div>}
        </div>
    );
}
