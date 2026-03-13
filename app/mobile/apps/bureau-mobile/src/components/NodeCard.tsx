import React from "react";
import { View, Text, TouchableOpacity, StyleSheet } from "react-native";
import type { NodeKindDefinition } from "@bureau/nodes-core";
import type { OfflineNodeDto } from "@bureau/offline-store";

type Props = {
    node: OfflineNodeDto;
    definition: NodeKindDefinition;
    onPress: () => void;
};

function getLabel(node: OfflineNodeDto): string {
    const attr = node.attributes.find((a) => a.key === "label");
    return attr?.valueString ?? "";
}

export function NodeCard({ node, definition, onPress }: Props): React.ReactElement {
    const label = getLabel(node);
    const syncStatus = node._syncStatus;

    return (
        <TouchableOpacity style={styles.card} onPress={onPress}>
            <View style={styles.headerRow}>
                {label ? <Text style={styles.title}>{label}</Text> : null}
                {syncStatus === "pending" && (
                    <View style={[styles.badge, styles.pendingBadge]}>
                        <Text style={styles.badgeText}>Pending</Text>
                    </View>
                )}
                {syncStatus === "failed" && (
                    <View style={[styles.badge, styles.failedBadge]}>
                        <Text style={styles.badgeText}>Failed</Text>
                    </View>
                )}
            </View>

            {definition.requiresCanonicalKey && node.canonicalKey ? (
                <Text style={styles.meta}>Key: {node.canonicalKey}</Text>
            ) : null}

            {definition.requiresScope && node.scope ? (
                <Text style={styles.meta}>Scope: {node.scope}</Text>
            ) : null}

            <Text style={styles.nodeId}>{node.nodeId}</Text>
        </TouchableOpacity>
    );
}

const styles = StyleSheet.create({
    card: {
        marginHorizontal: 12,
        marginVertical: 4,
        padding: 14,
        borderWidth: 1,
        borderColor: "#eee",
        borderRadius: 6,
        backgroundColor: "#fff",
    },
    headerRow: {
        flexDirection: "row",
        justifyContent: "space-between",
        alignItems: "center",
    },
    title: { fontSize: 16, fontWeight: "600", marginBottom: 2, flex: 1 },
    meta: { fontSize: 13, color: "#666" },
    nodeId: { fontSize: 12, color: "#999", fontFamily: "monospace", marginTop: 4 },
    badge: {
        paddingHorizontal: 8,
        paddingVertical: 2,
        borderRadius: 10,
        marginLeft: 8,
    },
    pendingBadge: { backgroundColor: "#f0ad4e" },
    failedBadge: { backgroundColor: "#d9534f" },
    badgeText: { color: "#fff", fontSize: 11, fontWeight: "600" },
});
