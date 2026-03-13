import React, { createContext } from "react";
import type { AppConfig } from "./AppConfig";

export const ConfigContext = createContext<AppConfig | null>(null);

interface ConfigProviderProps {
    config: AppConfig;
    children: React.ReactNode;
}

export function ConfigProvider({ config, children }: ConfigProviderProps): React.ReactElement {
    return <ConfigContext.Provider value={config}>{children}</ConfigContext.Provider>;
}
