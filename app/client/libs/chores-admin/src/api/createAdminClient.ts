import { ApiClient, AppRuntimeOptionsLoader } from "@bureau/client-core";
import type { AppRuntimeOptions } from "@bureau/client-core";
import { ChoresApi } from "@bureau/chores-core";

export function createAdminApi(): { api: ApiClient; choresApi: ChoresApi; runtime: AppRuntimeOptions } {
    const runtime = AppRuntimeOptionsLoader.loadFromWindow();
    const baseUrl = runtime.apiBaseUrls["chores"];
    if (!baseUrl) throw new Error("ChoresAdminApi: no 'chores' key in apiBaseUrls.");
    const api = new ApiClient(runtime);
    const choresApi = new ChoresApi(api, baseUrl);
    return { api, choresApi, runtime };
}
