import type { AppConfig } from "../config/AppConfig";
import type { AuthUser, IAuthProvider } from "./IAuthProvider";

export class DevAuthProvider implements IAuthProvider {
    readonly user: AuthUser;
    readonly featureScopes: string[];

    constructor(_config: AppConfig) {
        this.user = { name: "dev-user" };
        // In dev mode all feature scopes are active.
        // To simulate a restricted user, edit Auth:Dev:Features in appsettings.Development.json
        // (backend) or narrow this list locally.
        this.featureScopes = [
            "niles",
            "niles.chores",
            "watson",
            "watson.nodes",
            "watson.nodes.crud",
            "watson.nodes.analytics",
            "watson.items",
        ];
    }

    async getAccessTokenAsync(): Promise<string | null> {
        return "dev-token";
    }
}
