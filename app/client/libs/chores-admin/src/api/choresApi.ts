export { ChoresApi } from "@bureau/chores-core";
export type {
    ChoreDto,
    ChoreUpsert,
    CriticalPayload,
    HousekeepingChoreDto,
    HousekeepingLogDto,
    HousekeepingUpsert,
    SearchQuery,
} from "@bureau/chores-core";

import type { ChoresApi } from "@bureau/chores-core";
export type Props = {
    choresApi: ChoresApi;
};
