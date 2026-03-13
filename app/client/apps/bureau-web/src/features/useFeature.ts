import { useMemo } from "react";
import { useAuth } from "../auth/useAuth";
import { buildFeatureTree, isFeatureEnabled } from "./featureTree";

export function useFeature(path: string): boolean {
    const { featureScopes } = useAuth();
    const tree = useMemo(() => buildFeatureTree(featureScopes), [featureScopes]);
    return isFeatureEnabled(tree, path);
}
