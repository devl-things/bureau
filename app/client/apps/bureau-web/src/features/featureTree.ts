export type FeatureTree = boolean | { [key: string]: FeatureTree };

export function buildFeatureTree(scopes: string[]): FeatureTree {
    const tree: Record<string, FeatureTree> = {};
    for (const scope of scopes) {
        const parts = scope.split(".");
        let node = tree;
        for (let i = 0; i < parts.length; i++) {
            const part = parts[i];
            if (i === parts.length - 1) {
                node[part] = true;
            } else {
                if (!node[part] || typeof node[part] === "boolean") {
                    node[part] = {};
                }
                node = node[part] as Record<string, FeatureTree>;
            }
        }
    }
    return tree;
}

// Traverse tree by dot-path. Missing node or false at any level → false.
export function isFeatureEnabled(tree: FeatureTree, path: string): boolean {
    const parts = path.split(".");
    let node: FeatureTree = tree;
    for (const part of parts) {
        if (!node || node === true) return node === true;
        node = (node as Record<string, FeatureTree>)[part];
    }
    return !!node;
}
