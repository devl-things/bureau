// Cache the promise so /config is fetched only once per page load
let apiConfigPromise = null;
/**
 * @typedef {Object} ApiConfig
 * @property {string} chores
 * @property {string} prioritizedChores
 * @property {string} health
 * @property {string} housekeeping
 * @property {string} housekeepingSubmit
 */
/**
 * Load API config for the app.
 * - Fetches /config only once
 * - Maps it to a simple API object
 * - Stores it on window.API for convenience
 * - Returns a Promise that resolves to the API object
 * @returns {Promise<ApiConfig>}
 */
function loadApiConfig() {
    if (!apiConfigPromise) {
        apiConfigPromise = fetch("/config")
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Failed to load config: " + response.status);
                }
                return response.json();
            })
            .then(function (cfg) {
                // Map backend shape → frontend API shape
                const api = Object.freeze({
                    chores: cfg.api.choresEndpoint,
                    prioritizedChores: cfg.api.prioritizedChoresEndpoint,
                    health: cfg.api.healthEndpoint,
                    housekeeping: cfg.api.housekeepingEndpoint,
                    housekeepingSubmit: cfg.api.housekeepingSubmitEndpoint
                });

                // Expose for other scripts / debugging
                globalThis.API = api;
                return api;
            })
            .catch(function (err) {
                // Clear cached rejected promise so next call can retry
                apiConfigPromise = null;
                throw err;
            });
    }

    return apiConfigPromise;
}

// Expose the loader so other one-pagers can reuse it
globalThis.loadApiConfig = loadApiConfig;
