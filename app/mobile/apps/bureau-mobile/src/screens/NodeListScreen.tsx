import React, { useState } from "react";
import {
    View,
    Text,
    FlatList,
    TextInput,
    TouchableOpacity,
    StyleSheet,
    ActivityIndicator,
} from "react-native";
import type { NodeKindDefinition } from "@bureau/nodes-core";
import { useOfflineContext, useOfflineNodes } from "@bureau/offline-store";
import { NodeCard } from "../components/NodeCard";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import type { KindStackParamList } from "../navigation/AppNavigator";

type Props = NativeStackScreenProps<KindStackParamList, "List"> & {
    definition: NodeKindDefinition;
};

export function NodeListScreen({ navigation, definition }: Props): React.ReactElement {
    const { offlineApi, isOnline } = useOfflineContext();
    const [search, setSearch] = useState("");

    const { nodes, loading, hasMore, loadMore, refresh } = useOfflineNodes(
        offlineApi,
        definition,
        search
    );

    return (
        <View style={styles.container}>
            {!isOnline && (
                <View style={styles.offlineBanner}>
                    <Text style={styles.offlineBannerText}>Offline — showing cached data</Text>
                </View>
            )}

            <TextInput
                style={styles.searchInput}
                placeholder={`Search ${definition.pluralLabel.toLowerCase()}...`}
                value={search}
                onChangeText={setSearch}
                returnKeyType="search"
                onSubmitEditing={refresh}
            />

            <TouchableOpacity
                style={styles.createButton}
                onPress={() => navigation.navigate("Create")}
            >
                <Text style={styles.createButtonText}>+ Create {definition.label}</Text>
            </TouchableOpacity>

            <FlatList
                data={nodes}
                keyExtractor={(item) => item.nodeId}
                renderItem={({ item }) => (
                    <NodeCard
                        node={item}
                        definition={definition}
                        onPress={() => navigation.navigate("Detail", { nodeId: item.nodeId })}
                    />
                )}
                onEndReached={() => {
                    if (hasMore) loadMore();
                }}
                onEndReachedThreshold={0.5}
                ListEmptyComponent={
                    !loading ? (
                        <Text style={styles.empty}>No {definition.pluralLabel.toLowerCase()} found.</Text>
                    ) : null
                }
                ListFooterComponent={loading ? <ActivityIndicator style={{ padding: 16 }} /> : null}
            />
        </View>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: "#fff" },
    offlineBanner: {
        backgroundColor: "#f0ad4e",
        paddingVertical: 6,
        paddingHorizontal: 12,
        alignItems: "center",
    },
    offlineBannerText: { color: "#fff", fontWeight: "600", fontSize: 13 },
    searchInput: {
        margin: 12,
        padding: 10,
        borderWidth: 1,
        borderColor: "#ddd",
        borderRadius: 6,
        fontSize: 15,
    },
    createButton: {
        marginHorizontal: 12,
        marginBottom: 8,
        padding: 12,
        borderRadius: 6,
        borderWidth: 1,
        borderColor: "#ccc",
        alignItems: "center",
    },
    createButtonText: { fontWeight: "600", fontSize: 15 },
    empty: { textAlign: "center", padding: 24, color: "#999" },
});
