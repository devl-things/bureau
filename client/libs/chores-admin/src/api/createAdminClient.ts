import { ApiClient } from "@bureau/client-core";
import { AppRuntimeOptionsLoader } from "@bureau/client-core";
import type { AppRuntimeOptions } from "@bureau/client-core";
export function createAdminApi(): { api: ApiClient; runtime: AppRuntimeOptions } {
    const runtime = AppRuntimeOptionsLoader.loadFromWindow();
    const api = new ApiClient(runtime);
    return { api, runtime };
}
