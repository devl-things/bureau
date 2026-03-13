import { useContext } from "react";
import { ConfigContext } from "./ConfigContext";
import type { AppConfig } from "./AppConfig";

export function useConfig(): AppConfig {
    const config = useContext(ConfigContext);
    if (!config) throw new Error("useConfig must be used within ConfigProvider");
    return config;
}
