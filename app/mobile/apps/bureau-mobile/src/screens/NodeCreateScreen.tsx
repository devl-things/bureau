import React, { useState } from "react";
import { View, Text, ScrollView, TouchableOpacity, StyleSheet, Alert } from "react-native";
import {
    type NodeKindDefinition,
    type FormState,
    type FormErrors,
    buildEmptyForm,
    formToCreateRequest,
    validateForm,
} from "@bureau/nodes-core";
import { useOfflineContext } from "@bureau/offline-store";
import { NodeFormNative } from "../components/NodeForm.native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import type { KindStackParamList } from "../navigation/AppNavigator";

type Props = NativeStackScreenProps<KindStackParamList, "Create"> & {
    definition: NodeKindDefinition;
};

export function NodeCreateScreen({ navigation, definition }: Props): React.ReactElement {
    const { offlineApi } = useOfflineContext();

    const [form, setForm] = useState<FormState>(() => buildEmptyForm(definition));
    const [errors, setErrors] = useState<FormErrors>({});
    const [submitting, setSubmitting] = useState(false);

    async function handleSubmit(): Promise<void> {
        const validation = validateForm(definition, form);
        if (Object.keys(validation).length > 0) {
            setErrors(validation);
            return;
        }

        setErrors({});
        setSubmitting(true);

        try {
            const request = formToCreateRequest(definition, form);
            await offlineApi.createNode(definition, request);
            navigation.goBack();
        } catch (err) {
            Alert.alert("Error", err instanceof Error ? err.message : "Failed to create node.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <ScrollView style={styles.container}>
            <NodeFormNative
                definition={definition}
                form={form}
                errors={errors}
                onChange={setForm}
            />

            <View style={styles.actions}>
                <TouchableOpacity
                    style={[styles.button, styles.primaryButton]}
                    disabled={submitting}
                    onPress={() => void handleSubmit()}
                >
                    <Text style={styles.primaryButtonText}>
                        {submitting ? "Creating..." : `Create ${definition.label}`}
                    </Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.button} onPress={() => navigation.goBack()}>
                    <Text>Cancel</Text>
                </TouchableOpacity>
            </View>
        </ScrollView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: "#fff", padding: 16 },
    actions: { flexDirection: "row", gap: 10, marginTop: 16, marginBottom: 32 },
    button: {
        padding: 12,
        borderRadius: 6,
        borderWidth: 1,
        borderColor: "#ccc",
        alignItems: "center",
        flex: 1,
    },
    primaryButton: { backgroundColor: "#007AFF", borderColor: "#007AFF" },
    primaryButtonText: { color: "#fff", fontWeight: "600" },
});
