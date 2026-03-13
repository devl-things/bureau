import React from "react";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import {
    ItemDefinition,
    TagDefinition,
    VariantDefinition,
    ProjectDefinition,
    type NodeKindDefinition,
} from "@bureau/nodes-core";
import { NodeListScreen } from "../screens/NodeListScreen";
import { NodeCreateScreen } from "../screens/NodeCreateScreen";
import { NodeDetailScreen } from "../screens/NodeDetailScreen";

export type KindStackParamList = {
    List: undefined;
    Create: undefined;
    Detail: { nodeId: string };
};

const Tab = createBottomTabNavigator();

function createKindStack(definition: NodeKindDefinition): () => React.ReactElement {
    const Stack = createNativeStackNavigator<KindStackParamList>();

    return function KindStack(): React.ReactElement {
        return (
            <Stack.Navigator screenOptions={{ headerShown: true }}>
                <Stack.Screen
                    name="List"
                    options={{ title: definition.pluralLabel }}
                >
                    {(props) => <NodeListScreen {...props} definition={definition} />}
                </Stack.Screen>
                <Stack.Screen
                    name="Create"
                    options={{ title: `Create ${definition.label}` }}
                >
                    {(props) => <NodeCreateScreen {...props} definition={definition} />}
                </Stack.Screen>
                <Stack.Screen
                    name="Detail"
                    options={{ title: `${definition.label} Detail` }}
                >
                    {(props) => <NodeDetailScreen {...props} definition={definition} />}
                </Stack.Screen>
            </Stack.Navigator>
        );
    };
}

const ItemsStack = createKindStack(ItemDefinition);
const TagsStack = createKindStack(TagDefinition);
const VariantsStack = createKindStack(VariantDefinition);
const ProjectsStack = createKindStack(ProjectDefinition);

export function AppNavigator(): React.ReactElement {
    return (
        <Tab.Navigator screenOptions={{ headerShown: false }}>
            <Tab.Screen name="Items" component={ItemsStack} />
            <Tab.Screen name="Tags" component={TagsStack} />
            <Tab.Screen name="Variants" component={VariantsStack} />
            <Tab.Screen name="Projects" component={ProjectsStack} />
        </Tab.Navigator>
    );
}
