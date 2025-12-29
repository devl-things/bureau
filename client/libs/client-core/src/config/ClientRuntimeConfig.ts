export type ClientRuntimeConfig = {
    /**
     * e.g. "dev", "prod"
     */
    environment: string;

    /**
     * build/version string shown in UI
     */
    version: string;

    /**
     * Named base URLs, e.g. { choresApi: "https://...", sven: "https://..." }
     */
    apiBaseUrls: Record<string, string>;

    /**
     * Optional default key used by ApiClient when apiKey is omitted.
     */
    defaultApiKey?: string;
};
