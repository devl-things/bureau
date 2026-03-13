export type ChoreDto = {
    id: string;
    title: string;
    description?: string | null;
    type?: string | null;
    weeklyInterval?: number | null;
    isCritical?: boolean;
};

export type ChoreUpsert = {
    title: string;
    description: string | null;
    type: string;
    weeklyInterval: number;
};

export type CriticalPayload = {
    description: string;
};

export type PrioritizedChoreDto = {
    id: string;
    title: string;
    description?: string | null;
    type?: string | null;
    note?: string | null;
    criticality?: number | null;
    priority?: number | null;
    completed?: boolean;
};

export type HousekeepingChoreDto = {
    id?: string | null;
    title?: string | null;
};

export type HousekeepingLogDto = {
    id: string;
    datetime: string;
    duration: string;
    note?: string | null;
    completedChores?: HousekeepingChoreDto[] | null;
};

export type HousekeepingUpsert = {
    datetime: string;
    duration: string;
    note: string | null;
    completedChoreIds: string[];
};

export type SearchQuery = {
    page: number;
    pageSize: number;
    search?: string;
};
