export interface IAccessTokenProvider {
    /**
     * Returns an access token to call APIs, or null if not available.
     * This intentionally supports async so later OIDC flows can be plugged in.
     */
    getAccessTokenAsync(): Promise<string | null>;
}
