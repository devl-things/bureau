import React, { useCallback, useEffect, useState } from "react";
import { View, Text, ScrollView, TouchableOpacity, StyleSheet, Alert, ActivityIndicator } from "react-native";
import {
    type NodeKindDefinition,
    type FormState,
    type FormErrors,
    nodeToFormState,
    formToCreateRequest,
    validateForm,
} from "@bureau/nodes-core";
import { useOfflineContext } from "@bureau/offline-store";
import type { OfflineNodeDto } from "@bureau/offline-store";
import { NodeFormNative } from "../components/NodeForm.native";
import { EdgeListNative } from "../components/EdgeList.native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import type { KindStackParamList } from "../navigation/AppNavigator";

type Props = NativeStackScreenProps<KindStackParamList, "Detail"> & {
    definition: NodeKindDefinition;
};

export function NodeDetailScreen({ route, definition }: Props): React.ReactElement {
    const { nodeId } = route.params;
    const { offlineApi } = useOfflineContext();

    const [node, setNode] = useState<OfflineNodeDto | null>(null);
    const [form, setForm] = useState<FormState | null>(null);
    const [errors, setErrors] = useState<FormErrors>({});
    const [saving, setSaving] = useState(false);
    const [loading, setLoading] = useState(true);

    const loadNode = useCallback(async () => {
        setLoading(true);
        try {
            const n = await offlineApi.getNode(nodeId);
            setNode(n);
            setForm(nodeToFormState(definition, n));
        } catch (err) {
            Alert.alert("Error", err instanceof Error ? err.message : "Failed to load node.");
        } finally {
            setLoading(false);
        }
    }, [nodeId, offlineApi, definition]);

    useEffect(() => {
        void loadNode();
    }, [loadNode]);

    async function handleSave(): Promise<void> {
        if (!form || !node) return;

        const validation = validateForm(definition, form);
        if (Object.keys(validation).length > 0) {
            setErrors(validation);
            return;
        }

        setErrors({});
        setSaving(true);

        try {
            const request = formToCreateRequest(definition, form);
            await offlineApi.patchAttributes(node.nodeId, { set: request.attributes });
            Alert.alert("Saved", "Changes saved successfully.");
        } catch (err) {
            Alert.alert("Error", err instanceof Error ? err.message : "Failed to save.");
        } finally {
            setSaving(false);
        }
    }

    if (loading || !form || !node) {
        return (
            <View style={[styles.container, { justifyContent: "center", alignItems: "center" }]}>
                <ActivityIndicator size="large" />
            </View>
        );
    }

    return (
        <ScrollView style={styles.container}>
            <Text style={styles.metaText}>ID: {node.nodeId}</Text>
            <Text style={styles.metaText}>Version: {node.version}</Text>
            {node._syncStatus && node._syncStatus !== "synced" && (
                <Text style={[styles.metaText, { color: node._syncStatus === "failed" ? "#d9534f" : "#f0ad4e" }]}>
                    Sync: {node._syncStatus}
                </Text>
            )}

            <NodeFormNative
                definition={definition}
                form={form}
                errors={errors}
                onChange={setForm}
            />

            <TouchableOpacity
                style={[styles.button, styles.primaryButton]}
                disabled={saving}
                onPress={() => void handleSave()}
            >
                <Text style={styles.primaryButtonText}>
                    {saving ? "Saving..." : "Save Changes"}
                </Text>
            </TouchableOpacity>

            <EdgeListNative nodeId={node.nodeId} />
        </ScrollView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: "#fff", padding: 16 },
    metaText: { color: "#888", fontSize: 13, marginBottom: 4, fontFamily: "monospace" },
    button: {
        padding: 12,
        borderRadius: 6,
        borderWidth: 1,
        borderColor: "#ccc",
        alignItems: "center",
        marginTop: 16,
    },
    primaryButton: { backgroundColor: "#007AFF", borderColor: "#007AFF" },
    primaryButtonText: { color: "#fff", fontWeight: "600" },
});
