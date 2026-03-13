import React from "react";

interface Props {
    name: string;
}

export function FeatureUnavailable({ name }: Props): React.ReactElement {
    return (
        <div style={{ padding: "2rem", color: "var(--muted)" }}>
            <strong>{name}</strong> is not available.
        </div>
    );
}
