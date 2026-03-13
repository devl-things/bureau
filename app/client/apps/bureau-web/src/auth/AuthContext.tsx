import React, { createContext, useMemo } from "react";
import type { AppConfig } from "../config/AppConfig";
import type { IAuthProvider } from "./IAuthProvider";
import { DevAuthProvider } from "./DevAuthProvider";
import { OidcAuthProvider } from "./OidcAuthProvider";

export const AuthContext = createContext<IAuthProvider | null>(null);

interface AuthProviderProps {
    config: AppConfig;
    children: React.ReactNode;
}

export function AuthProvider({ config, children }: AuthProviderProps): React.ReactElement {
    const provider = useMemo<IAuthProvider>(() => {
        return config.auth.mode === "oidc"
            ? new OidcAuthProvider()
            : new DevAuthProvider(config);
    }, [config]);

    return <AuthContext.Provider value={provider}>{children}</AuthContext.Provider>;
}
