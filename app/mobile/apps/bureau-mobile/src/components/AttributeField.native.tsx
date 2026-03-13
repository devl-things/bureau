import React from "react";
import { View, Text, TextInput, Switch, StyleSheet } from "react-native";
import { AttributeValueType, type AttributeFieldDefinition } from "@bureau/nodes-core";

type Props = {
    field: AttributeFieldDefinition;
    value: string;
    onChange: (value: string) => void;
    error?: string;
};

export function AttributeFieldNative({ field, value, onChange, error }: Props): React.ReactElement {
    switch (field.type) {
        case AttributeValueType.Bool:
            return (
                <View style={styles.field}>
                    <View style={styles.switchRow}>
                        <Text style={styles.label}>{field.label}</Text>
                        <Switch
                            value={value === "true"}
                            onValueChange={(v) => onChange(String(v))}
                        />
                    </View>
                    {error ? <Text style={styles.error}>{error}</Text> : null}
                </View>
            );

        case AttributeValueType.Number:
            return (
                <View style={styles.field}>
                    <Text style={styles.label}>{field.label}</Text>
                    <TextInput
                        style={styles.input}
                        value={value}
                        placeholder={field.placeholder}
                        keyboardType="numeric"
                        onChangeText={onChange}
                    />
                    {error ? <Text style={styles.error}>{error}</Text> : null}
                </View>
            );

        case AttributeValueType.Json:
            return (
                <View style={styles.field}>
                    <Text style={styles.label}>{field.label}</Text>
                    <TextInput
                        style={[styles.input, styles.textarea]}
                        value={value}
                        placeholder={field.placeholder ?? "JSON"}
                        multiline
                        onChangeText={onChange}
                    />
                    {error ? <Text style={styles.error}>{error}</Text> : null}
                </View>
            );

        default:
            return (
                <View style={styles.field}>
                    <Text style={styles.label}>{field.label}</Text>
                    <TextInput
                        style={styles.input}
                        value={value}
                        placeholder={field.placeholder}
                        onChangeText={onChange}
                    />
                    {error ? <Text style={styles.error}>{error}</Text> : null}
                </View>
            );
    }
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
    textarea: { minHeight: 100, textAlignVertical: "top" },
    switchRow: { flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
    error: { color: "#b54a4a", fontSize: 12, marginTop: 2 },
});
