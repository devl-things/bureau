export interface AppConfig {
    environment: string;
    auth: {
        mode: "dev" | "oidc";
        oidc?: {
            authority: string;
            clientId: string;
            redirectUri: string;
        };
    };
    apis: {
        chores: string;
        nodes: string;
        itemsIngest: string;
    };
}
