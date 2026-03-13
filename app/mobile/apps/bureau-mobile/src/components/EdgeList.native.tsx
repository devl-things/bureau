import React, { useCallback, useEffect, useState } from "react";
import { View, Text, TextInput, TouchableOpacity, StyleSheet, Alert } from "react-native";
import type { EdgeDto } from "@bureau/nodes-core";
import { useOfflineContext } from "@bureau/offline-store";

type Props = {
    nodeId: string;
};

export function EdgeListNative({ nodeId }: Props): React.ReactElement {
    const { offlineApi } = useOfflineContext();

    const [edges, setEdges] = useState<EdgeDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [purpose, setPurpose] = useState("");
    const [targetNodeId, setTargetNodeId] = useState("");
    const [orderIndex, setOrderIndex] = useState("0");

    const loadEdges = useCallback(async () => {
        setLoading(true);
        try {
            const result = await offlineApi.getEdges(nodeId);
            setEdges(result.data);
        } catch {
            setEdges([]);
        } finally {
            setLoading(false);
        }
    }, [nodeId, offlineApi]);

    useEffect(() => {
        void loadEdges();
    }, [loadEdges]);

    async function handleAdd(): Promise<void> {
        if (!targetNodeId.trim() || !purpose.trim()) {
            Alert.alert("Validation", "Target node ID and purpose are required.");
            return;
        }
        try {
            await offlineApi.addEdge(nodeId, {
                targetNodeId: targetNodeId.trim(),
                purpose: purpose.trim(),
                orderIndex: parseInt(orderIndex, 10) || 0,
            });
            setTargetNodeId("");
            setPurpose("");
            setOrderIndex("0");
            await loadEdges();
        } catch (e) {
            Alert.alert("Error", e instanceof Error ? e.message : "Failed to add edge.");
        }
    }

    async function handleRemove(edge: EdgeDto): Promise<void> {
        try {
            await offlineApi.removeEdge(nodeId, {
                targetNodeId: edge.targetNodeId,
                purpose: edge.purpose,
            });
            await loadEdges();
        } catch (e) {
            Alert.alert("Error", e instanceof Error ? e.message : "Failed to remove edge.");
        }
    }

    return (
        <View style={styles.container}>
            <Text style={styles.heading}>Edges</Text>

            {loading && <Text style={styles.muted}>Loading edges...</Text>}

            {!loading && edges.length === 0 && <Text style={styles.muted}>No edges.</Text>}

            {edges.map((edge, i) => (
                <View key={`${edge.targetNodeId}-${edge.purpose}-${i}`} style={styles.edgeRow}>
                    <View style={{ flex: 1 }}>
                        <Text style={styles.edgeTarget}>{edge.targetNodeId}</Text>
                        <Text style={styles.edgeMeta}>
                            {edge.purpose} (order: {edge.orderIndex})
                        </Text>
                    </View>
                    <TouchableOpacity onPress={() => void handleRemove(edge)}>
                        <Text style={styles.removeText}>Remove</Text>
                    </TouchableOpacity>
                </View>
            ))}

            <View style={styles.addSection}>
                <TextInput
                    style={styles.input}
                    placeholder="Target node ID"
                    value={targetNodeId}
                    onChangeText={setTargetNodeId}
                />
                <TextInput
                    style={styles.input}
                    placeholder="Purpose"
                    value={purpose}
                    onChangeText={setPurpose}
                />
                <TextInput
                    style={[styles.input, { width: 80 }]}
                    placeholder="Order"
                    value={orderIndex}
                    keyboardType="numeric"
                    onChangeText={setOrderIndex}
                />
                <TouchableOpacity style={styles.addButton} onPress={() => void handleAdd()}>
                    <Text style={{ fontWeight: "600" }}>Add Edge</Text>
                </TouchableOpacity>
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    container: { marginTop: 16, marginBottom: 32 },
    heading: { fontSize: 16, fontWeight: "600", marginBottom: 10 },
    muted: { color: "#999", fontSize: 14 },
    edgeRow: {
        flexDirection: "row",
        alignItems: "center",
        padding: 10,
        borderWidth: 1,
        borderColor: "#eee",
        borderRadius: 6,
        marginBottom: 6,
    },
    edgeTarget: { fontFamily: "monospace", fontSize: 12 },
    edgeMeta: { fontSize: 13, color: "#666" },
    removeText: { color: "#b54a4a", fontWeight: "600", fontSize: 13 },
    addSection: { marginTop: 10, gap: 8 },
    input: {
        padding: 10,
        borderWidth: 1,
        borderColor: "#ddd",
        borderRadius: 6,
        fontSize: 15,
        backgroundColor: "#fafafa",
    },
    addButton: {
        padding: 12,
        borderWidth: 1,
        borderColor: "#ccc",
        borderRadius: 6,
        alignItems: "center",
    },
});
