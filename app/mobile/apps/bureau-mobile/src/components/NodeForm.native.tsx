import React from "react";
import { View, Text, TextInput, TouchableOpacity, StyleSheet } from "react-native";
import {
    AttributeCardinality,
    type NodeKindDefinition,
    type FormState,
    type FormErrors,
} from "@bureau/nodes-core";
import { AttributeFieldNative } from "./AttributeField.native";

type Props = {
    definition: NodeKindDefinition;
    form: FormState;
    errors: FormErrors;
    onChange: (form: FormState) => void;
};

export function NodeFormNative({ definition, form, errors, onChange }: Props): React.ReactElement {
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
        <View>
            {definition.requiresCanonicalKey && (
                <View style={styles.field}>
                    <Text style={styles.label}>Canonical Key</Text>
                    <TextInput
                        style={styles.input}
                        value={form.canonicalKey}
                        placeholder="Unique identifier"
                        onChangeText={(v) => onChange({ ...form, canonicalKey: v })}
                    />
                    {errors["canonicalKey"] ? (
                        <Text style={styles.error}>{errors["canonicalKey"]}</Text>
                    ) : null}
                </View>
            )}

            {definition.requiresScope && (
                <View style={styles.field}>
                    <Text style={styles.label}>Scope</Text>
                    <TextInput
                        style={styles.input}
                        value={form.scope}
                        placeholder="Scope"
                        onChangeText={(v) => onChange({ ...form, scope: v })}
                    />
                    {errors["scope"] ? <Text style={styles.error}>{errors["scope"]}</Text> : null}
                </View>
            )}

            {definition.attributes.map((field) => {
                if (field.cardinality === AttributeCardinality.Single) {
                    const value = form.attributes[field.key];
                    return (
                        <AttributeFieldNative
                            key={field.key}
                            field={field}
                            value={typeof value === "string" ? value : ""}
                            onChange={(v) => setSingleAttribute(field.key, v)}
                            error={errors[field.key]}
                        />
                    );
                }

                const values = Array.isArray(form.attributes[field.key])
                    ? (form.attributes[field.key] as string[])
                    : [];

                return (
                    <View key={field.key} style={styles.field}>
                        <Text style={styles.label}>{field.label}</Text>
                        {values.map((val, i) => (
                            <View key={i} style={styles.manyRow}>
                                <View style={{ flex: 1 }}>
                                    <AttributeFieldNative
                                        field={field}
                                        value={val}
                                        onChange={(v) => setManyAttribute(field.key, i, v)}
                                    />
                                </View>
                                <TouchableOpacity
                                    style={styles.removeButton}
                                    onPress={() => removeManyEntry(field.key, i)}
                                >
                                    <Text style={styles.removeText}>Remove</Text>
                                </TouchableOpacity>
                            </View>
                        ))}
                        <TouchableOpacity
                            style={styles.addButton}
                            onPress={() => addManyEntry(field.key)}
                        >
                            <Text>+ Add {field.label}</Text>
                        </TouchableOpacity>
                        {errors[field.key] ? (
                            <Text style={styles.error}>{errors[field.key]}</Text>
                        ) : null}
                    </View>
                );
            })}
        </View>
    );
}

const styles = StyleSheet.create({
    field: { marginBottom: 12 },
    label: { fontSize: 13, fontWeight: "600", color: "#666", marginBottom: 4 },
    input: {
        padding: 10,
        borderWidth: 1,
        borderColor: "#ddd",
        borderRadius: 6,
        fontSize: 15,
        backgroundColor: "#fafafa",
    },
    error: { color: "#b54a4a", fontSize: 12, marginTop: 2 },
    manyRow: { flexDirection: "row", alignItems: "center", gap: 8, marginBottom: 4 },
    removeButton: { padding: 8 },
    removeText: { color: "#b54a4a", fontSize: 13, fontWeight: "600" },
    addButton: {
        padding: 10,
        borderWidth: 1,
        borderColor: "#ddd",
        borderRadius: 6,
        alignItems: "center",
        marginTop: 4,
    },
});
