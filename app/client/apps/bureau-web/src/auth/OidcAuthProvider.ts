// Stub — wire up oidc-client-ts when Sven (feature/sven) is merged.
import type { AuthUser, IAuthProvider } from "./IAuthProvider";

export class OidcAuthProvider implements IAuthProvider {
    readonly user: AuthUser | null = null;
    readonly featureScopes: string[] = [];

    async getAccessTokenAsync(): Promise<string | null> {
        return null;
    }
}
