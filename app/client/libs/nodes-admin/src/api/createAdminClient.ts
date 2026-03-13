import { ApiClient, AppRuntimeOptionsLoader } from "@bureau/client-core";
import type { AppRuntimeOptions } from "@bureau/client-core";
import { NodesApi } from "@bureau/nodes-core";

export function createAdminApi(): { api: ApiClient; nodesApi: NodesApi; runtime: AppRuntimeOptions } {
    const runtime = AppRuntimeOptionsLoader.loadFromWindow();
    const api = new ApiClient(runtime);
    const nodesApi = new NodesApi(api, runtime);
    return { api, nodesApi, runtime };
}
