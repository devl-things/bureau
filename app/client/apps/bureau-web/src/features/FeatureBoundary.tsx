import React, { Component, Suspense } from "react";
import { useFeature } from "./useFeature";

interface ErrorBoundaryState {
    hasError: boolean;
}

class FeatureErrorBoundary extends Component<
    { fallback: React.ReactNode; children: React.ReactNode },
    ErrorBoundaryState
> {
    state: ErrorBoundaryState = { hasError: false };

    static getDerivedStateFromError(): ErrorBoundaryState {
        return { hasError: true };
    }

    render(): React.ReactNode {
        if (this.state.hasError) return this.props.fallback;
        return this.props.children;
    }
}

interface Props {
    feature: string;
    fallback?: React.ReactNode;
    children: React.ReactNode;
}

export function FeatureBoundary({ feature, fallback, children }: Props): React.ReactElement {
    const enabled = useFeature(feature);

    if (!enabled) return <>{fallback ?? null}</>;

    const errorFallback = fallback ?? (
        <div style={{ padding: "2rem", color: "var(--muted)" }}>Feature unavailable.</div>
    );

    return (
        <FeatureErrorBoundary fallback={errorFallback}>
            <Suspense fallback={<div style={{ padding: "2rem", color: "var(--muted)" }}>Loading…</div>}>
                {children}
            </Suspense>
        </FeatureErrorBoundary>
    );
}
